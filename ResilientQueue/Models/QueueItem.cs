using System.Diagnostics.CodeAnalysis;

namespace ResilientQueue.Models;

public class QueueItem
{
    [MemberNotNullWhen(true, nameof(System.Exception))]
    public bool Failed { internal get; set; }

    public int RetryCount { internal get; set; }

    public Exception? Exception { internal get; set; }
}