namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class WorkerCommandDecorator(WorkerCommandHandler inner, WorkerUnit unit)
    : ICommandHandler<WorkerRequest, Guid>
{
    public Task<Guid> HandleAsync(WorkerRequest request, CancellationToken cancellationToken = default)
    {
        unit.Decorated();
        return inner.HandleAsync(request, cancellationToken);
    }
}
