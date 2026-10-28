namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class ProbeHandler<TResult>(Func<ProbeRequest<TResult>, CancellationToken, Task<TResult>> execute)
    : ICommandHandler<ProbeRequest<TResult>, TResult>, IQueryHandler<ProbeRequest<TResult>, TResult>, IDisposable
{
    private int calls;
    public int Calls => calls;
    public bool Disposed { get; private set; }

    public Task<TResult> HandleAsync(ProbeRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref calls);
        return execute(request, cancellationToken);
    }

    public void Dispose() => Disposed = true;
}
