using ResilientQueue;
using ResilientQueue.Example;
using ResilientQueue.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<ResilientQueueConfiguration>()
    .Bind(builder.Configuration.GetRequiredSection("Queue"))
    .ValidateOnStart();

builder.Services.AddSerilog((_, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddQueueProcessor<ExampleQueueProcessor, ExampleItem>();

// service to pump fake items to the processor
builder.Services.AddHostedService<ExampleItemPumper>();

var host = builder.Build();
await host.RunAsync();