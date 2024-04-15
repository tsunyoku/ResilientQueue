using Microsoft.Extensions.Options;

namespace ResilientQueue.Example;

public class ExampleQueueProcessor(
    IOptions<ResilientQueueConfiguration> queueConfiguration,
    ILogger<ExampleQueueProcessor> logger)
    : ResilientQueueProcessor<ExampleItem>(queueConfiguration.Value, logger)
{
    protected override Task ProcessItemAsync(ExampleItem item)
    {
        // simulate 50% chance of fail to show resilience
        if (Random.Shared.Next(0, 2) != 0)
        {
            throw new Exception("Simulated failure");
        }

        logger.LogInformation("Processed {@Item}", item);
        return Task.CompletedTask;
    }
}