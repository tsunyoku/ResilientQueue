using Microsoft.Extensions.DependencyInjection;
using ResilientQueue.Models;

namespace ResilientQueue.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueueProcessor<T1, T2>(this IServiceCollection services)
        where T1 : ResilientQueueProcessor<T2>
        where T2 : QueueItem
    {
        services.AddSingleton<ResilientQueueProcessor<T2>, T1>();
        services.AddHostedService<ResilientQueueProcessorRunner<T2>>();

        return services;
    }
}