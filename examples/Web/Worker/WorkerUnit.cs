namespace Monstertacle.SharpQrs.Examples.Web.Worker;

public sealed class WorkerUnit : IAsyncDisposable
{
    internal Guid Id { get; } = Guid.NewGuid();
    internal int Disposals { get; private set; }
    internal int Decorations { get; private set; }
    internal void Decorated() => Decorations++;
    public ValueTask DisposeAsync()
    {
        Disposals++;
        return ValueTask.CompletedTask;
    }
}
