namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class SyncProbeHandler<TResult>(Func<ProbeRequest<TResult>, CancellationToken, TResult> execute)
    : ISyncCommandHandler<ProbeRequest<TResult>, TResult>, ISyncQueryHandler<ProbeRequest<TResult>, TResult>, IDisposable
{
    private int calls;
    public int Calls => calls;
    public bool Disposed { get; private set; }
    public TResult Handle(ProbeRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref calls);
        return execute(request, cancellationToken);
    }
    public void Dispose() => Disposed = true;
}
