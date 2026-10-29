namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed class WorkerResults
{
    private readonly TaskCompletionSource<IReadOnlyList<WorkerObservation>> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal void Complete(IReadOnlyList<WorkerObservation> observations) => completion.TrySetResult(observations);
    internal void Fail(Exception error) => completion.TrySetException(error);
    internal Task<IReadOnlyList<WorkerObservation>> WaitAsync(CancellationToken token) => completion.Task.WaitAsync(token);
}
