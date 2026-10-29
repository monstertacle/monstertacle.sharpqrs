namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class SyncScopeHandler : ISyncCommandHandler<ProbeRequest<Guid>, Guid>,
    ISyncQueryHandler<ProbeRequest<Guid>, Guid>, ICommandHandler<ProbeRequest<Guid>, Guid>, IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Disposals { get; private set; }
    public Guid Handle(ProbeRequest<Guid> request, CancellationToken cancellationToken = default) => Id;
    public Task<Guid> HandleAsync(ProbeRequest<Guid> request, CancellationToken cancellationToken = default) => Task.FromResult(Id);
    public void Dispose() => Disposals++;
}
