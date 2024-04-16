using System.ComponentModel.DataAnnotations;

namespace ResilientQueue;

public class ResilientQueueConfiguration
{
    [Required]
    public required string RedisHost { get; init; }

    [Required]
    public required string QueueName { get; init; }
    
    [Required]
    public required int MaxRetryCount { get; init; }
    
    [Required]
    public required int MaxBatchSize { get; init; }
    
    [Required]
    public required bool ProcessBatchInOrder { get; init; }
}