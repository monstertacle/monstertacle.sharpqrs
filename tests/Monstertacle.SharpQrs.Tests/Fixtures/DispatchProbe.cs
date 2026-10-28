namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class DispatchProbe<TResult>
{
    private readonly CommandDispatcher commands = new();
    private readonly QueryDispatcher queries = new();
    private readonly bool query;

    public DispatchProbe(bool query, ProbeHandler<TResult> handler)
    {
        this.query = query;
        commands.Register(handler);
        queries.Register(handler);
    }

    public Task<TResult> DispatchAsync(ProbeRequest<TResult> request, CancellationToken token = default)
        => query ? queries.DispatchAsync(request, token) : commands.DispatchAsync(request, token);
}
