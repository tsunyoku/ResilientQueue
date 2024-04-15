namespace ResilientQueue.Example;

public class ExampleItemPumper(
    ResilientQueueProcessor<ExampleItem> processor,
    ILogger<ExampleItemPumper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = new ExampleItem();
            await processor.PushItemToQueueAsync(item);
            
            logger.LogInformation("Pushed item to queue");

            await Task.Delay(TimeSpan.FromMilliseconds(50), stoppingToken);
        }
    }
}