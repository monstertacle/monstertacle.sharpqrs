namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class SyncDispatch(bool query)
{
    private readonly SyncCommandDispatcher commands = new();
    private readonly SyncQueryDispatcher queries = new();
    public void Register<TResult>(SyncProbeHandler<TResult> handler)
    {
        if (query) ((ISyncQueryRegistration)queries).Register(handler);
        else ((ISyncCommandRegistration)commands).Register(handler);
    }
    public TResult Dispatch<TResult>(ProbeRequest<TResult> request, CancellationToken token = default)
        => query ? queries.Dispatch(request, token) : commands.Dispatch(request, token);
}
