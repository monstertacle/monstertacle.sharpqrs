namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class CompositionHandler : ICommandHandler<ProbeRequest<int>, int>, IQueryHandler<ProbeRequest<int>, int>,
    ISyncCommandHandler<ProbeRequest<int>, int>, ISyncQueryHandler<ProbeRequest<int>, int>, IDisposable
{
    public int Disposals { get; private set; }
    public Task<int> HandleAsync(ProbeRequest<int> request, CancellationToken cancellationToken = default) => Task.FromResult(42);
    public int Handle(ProbeRequest<int> request, CancellationToken cancellationToken = default) => 42;
    public void Dispose() => Disposals++;
}
