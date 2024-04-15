using Microsoft.Extensions.Hosting;
using ResilientQueue.Models;

namespace ResilientQueue;

public class ResilientQueueProcessorRunner<T>(ResilientQueueProcessor<T> processor) : BackgroundService
    where T : QueueItem
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        await processor.RunAsync(stoppingToken);
    }
}