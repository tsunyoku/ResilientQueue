using System.Text.Json;
using Microsoft.Extensions.Logging;
using ResilientQueue.Models;
using StackExchange.Redis;

namespace ResilientQueue;

public abstract class ResilientQueueProcessor<T>(
    ResilientQueueConfiguration queueConfiguration,
    ILogger<ResilientQueueProcessor<T>> logger)
    where T : QueueItem
{
    private ConnectionMultiplexer? _connectionMultiplexer;

    public string QueueName { get; } = $"queue:{queueConfiguration.QueueName}";
    public string DeadletterQueueName => $"{QueueName}:deadletter";

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var cts = new GracefulShutdownSource(cancellationToken);

        var database = GetRedisDatabase();

        while (!cts.IsCancellationRequested)
        {
            var redisItems = await database.ListRightPopAsync(
                QueueName,
                queueConfiguration.MaxBatchSize);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (redisItems is null)
                continue;

            var items = redisItems
                .Where(i => !i.IsNullOrEmpty)
                .Select(redisItem => JsonSerializer.Deserialize<T>(redisItem!))
                .OfType<T>()
                .ToList();

            await ProcessItemsAsync(items);
            await RetryFailedItemsAsync(items.Where(i => i.Failed));
        }
    }

    protected abstract Task ProcessItemAsync(T item);
    
    private async Task ProcessItemsAsync(IEnumerable<T> items)
    {
        // TODO: replace with Task.WhenEach in .NET 9

        var tasks = items.Select(ProcessItemInternalAsync).ToList();

        while (tasks.Count != 0)
        {
            var finishedTask = await Task.WhenAny(tasks);
            tasks.Remove(finishedTask);
        }
    }

    private async Task ProcessItemInternalAsync(T item)
    {
        try
        {
            await ProcessItemAsync(item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process {@Item} in {@Queue}", item, QueueName);
            item.Failed = true;
            item.Exception = ex;
        }
    }

    private async Task RetryFailedItemsAsync(IEnumerable<T> failedItems)
    {
        foreach (var item in failedItems)
        {
            if (item.RetryCount >= queueConfiguration.MaxRetryCount)
            {
                await PushItemToDeadletterQueueAsync(item);
                continue;
            }

            item.Failed = false;
            item.RetryCount++;

            await PushItemToQueueAsync(item);
        }
    }

    public async Task PushItemToQueueAsync(T item) => await PushItemAsync(item, QueueName);

    private async Task PushItemToDeadletterQueueAsync(T item)
    {
        logger.LogError(
            "Pushing {@Item} to deadletter {@Queue}",
            item,
            DeadletterQueueName);
        
        await PushItemAsync(item, DeadletterQueueName);
    }

    private async Task PushItemAsync(T item, string queue)
    {
        var database = GetRedisDatabase();
        await database.ListLeftPushAsync(
            queue,
            JsonSerializer.Serialize(item));
    }

    private IDatabase GetRedisDatabase()
    {
        _connectionMultiplexer ??= ConnectionMultiplexer.Connect(queueConfiguration.RedisHost);
        return _connectionMultiplexer.GetDatabase();
    }
}