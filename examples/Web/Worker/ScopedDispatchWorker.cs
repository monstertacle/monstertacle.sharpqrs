namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class ScopedDispatchWorker(IServiceScopeFactory scopes, WorkerResults results) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var observations = new List<WorkerObservation>();
            for (var index = 0; index < 2; index++)
                observations.Add(await ProcessUnitAsync(stoppingToken));
            results.Complete(observations);
        }
        catch (Exception error)
        {
            results.Fail(error);
            throw;
        }
    }

    private async Task<WorkerObservation> ProcessUnitAsync(CancellationToken token)
    {
        WorkerUnit unit;
        (Guid Command, Guid Query) observed;
        await using (var scope = scopes.CreateAsyncScope())
        {
            unit = scope.ServiceProvider.GetRequiredService<WorkerUnit>();
            observed = await scope.ServiceProvider.GetRequiredService<WorkerOperation>().ExecuteAsync(token);
        }
        // Read diagnostic counters only; never reuse a service after its scope ends.
        return new WorkerObservation(observed.Command, observed.Query, unit.Disposals, unit.Decorations);
    }
}
