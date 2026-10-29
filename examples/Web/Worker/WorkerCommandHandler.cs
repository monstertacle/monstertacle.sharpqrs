namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class WorkerCommandHandler(WorkerUnit unit) : ICommandHandler<WorkerRequest, Guid>
{
    public Task<Guid> HandleAsync(WorkerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(unit.Id);
    }
}
