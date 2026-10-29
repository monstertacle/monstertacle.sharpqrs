namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class AsyncSyncHandler : ISyncCommandHandler<ProbeRequest<int>, int>,
    ISyncQueryHandler<ProbeRequest<int>, int>, IAsyncDisposable
{
    public int Disposals { get; private set; }
    public int Handle(ProbeRequest<int> request, CancellationToken cancellationToken = default) => 42;
    public ValueTask DisposeAsync()
    {
        Disposals++;
        return ValueTask.CompletedTask;
    }
}
