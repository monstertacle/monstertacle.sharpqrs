using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures;

public static class TestServices
{
    public static IServiceCollection AddEcho(this IServiceCollection services, bool query)
        => query ? services.AddSharpQrsQuery<EchoQuery, EchoResult, EchoQueryHandler>()
            : services.AddSharpQrsCommand<EchoCommand, EchoResult, EchoCommandHandler>();

    public static ServiceProvider BuildValidated(this IServiceCollection services)
        => services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

    public static Task<EchoResult> DispatchEcho(this IServiceProvider provider, bool query)
        => query ? provider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new EchoQuery("echo"))
            : provider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new EchoCommand("echo"));
}
