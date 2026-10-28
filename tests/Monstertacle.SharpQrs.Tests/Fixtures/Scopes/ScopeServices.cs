using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

public static class ScopeServices
{
    public static IServiceCollection AddScopedProbe(this IServiceCollection services)
    {
        services.AddScoped<ScopeState>();
        services.AddSharpQrsCommand<ScopeCommand, Guid, ScopedHandler>();
        services.AddSharpQrsQuery<ScopeQuery, Guid, ScopedHandler>();
        return services;
    }

    public static Task<Guid> DispatchScope(this IServiceProvider provider, bool query)
        => query ? provider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new ScopeQuery())
            : provider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new ScopeCommand());
}
