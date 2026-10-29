namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class WorkerOperation(ICommandDispatcher commands, IQueryDispatcher queries)
{
    internal async Task<(Guid Command, Guid Query)> ExecuteAsync(CancellationToken token)
    {
        var command = await commands.DispatchAsync(new WorkerRequest(), token);
        var query = await queries.DispatchAsync(new WorkerRequest(), token);
        return (command, query);
    }
}
