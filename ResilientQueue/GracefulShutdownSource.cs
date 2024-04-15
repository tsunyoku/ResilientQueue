using System.Runtime.Loader;

namespace ResilientQueue;

public class GracefulShutdownSource : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly ManualResetEventSlim _shutdownEvent;

    public bool IsCancellationRequested => _cts.IsCancellationRequested;

    public GracefulShutdownSource(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _shutdownEvent = new ManualResetEventSlim();

        AssemblyLoadContext.Default.Unloading += OnUnload;
        Console.CancelKeyPress += OnCancel;
    }

    private void Cancel() => _cts.Cancel();

    private void OnCancel(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        Cancel();
    }

    private void OnUnload(AssemblyLoadContext _)
    {
        Cancel();
        _shutdownEvent.Wait();
    }

    public void Dispose()
    {
        _shutdownEvent.Set();
        
        // don't unset Unloading since it will get called post-dispose
        Console.CancelKeyPress -= OnCancel;
        
        GC.SuppressFinalize(this);
    }
}