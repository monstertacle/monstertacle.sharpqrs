using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class DispatchEnvironment<TResult> : IDisposable
{
    private readonly Func<ProbeRequest<TResult>, CancellationToken, Task<TResult>> dispatch;
    private readonly ServiceProvider? provider;
    private readonly IServiceScope? scope;
    public ProbeHandler<TResult> Handler { get; }

    public DispatchEnvironment(bool query, bool integrated, Func<ProbeRequest<TResult>, CancellationToken, Task<TResult>> execute)
    {
        if (integrated)
        {
            var services = new ServiceCollection();
            services.AddSingleton(execute);
            services.AddSharpQrsCommand<ProbeRequest<TResult>, TResult, ProbeHandler<TResult>>();
            services.AddSharpQrsQuery<ProbeRequest<TResult>, TResult, ProbeHandler<TResult>>();
            provider = services.BuildValidated();
            scope = provider.CreateScope();
            Handler = scope.ServiceProvider.GetRequiredService<ProbeHandler<TResult>>();
            var commands = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
            var queries = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
            dispatch = query ? (request, token) => queries.DispatchAsync(request, token)
                : (request, token) => commands.DispatchAsync(request, token);
        }
        else
        {
            Handler = new ProbeHandler<TResult>(execute);
            var probe = new DispatchProbe<TResult>(query, Handler);
            dispatch = probe.DispatchAsync;
        }
    }

    public Task<TResult> DispatchAsync(ProbeRequest<TResult> request, CancellationToken token = default) => dispatch(request, token);

    public void Dispose()
    {
        if (scope is null) Handler.Dispose();
        scope?.Dispose();
        provider?.Dispose();
    }
}
