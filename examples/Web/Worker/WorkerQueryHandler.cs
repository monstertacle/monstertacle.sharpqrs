namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class WorkerQueryHandler(WorkerUnit unit) : IQueryHandler<WorkerRequest, Guid>
{
    public Task<Guid> HandleAsync(WorkerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(unit.Id);
    }
}
