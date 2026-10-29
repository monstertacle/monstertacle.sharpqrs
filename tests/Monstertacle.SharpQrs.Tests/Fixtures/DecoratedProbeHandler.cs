namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class DecoratedProbeHandler(ProbeHandler<int> inner)
    : ICommandHandler<ProbeRequest<int>, int>, IQueryHandler<ProbeRequest<int>, int>, IAsyncDisposable
{
    public int Calls { get; private set; }
    public int Disposals { get; private set; }
    public async Task<int> HandleAsync(ProbeRequest<int> request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return 1 + await inner.HandleAsync(request, cancellationToken);
    }
    public ValueTask DisposeAsync()
    {
        Disposals++;
        return ValueTask.CompletedTask;
    }
}
