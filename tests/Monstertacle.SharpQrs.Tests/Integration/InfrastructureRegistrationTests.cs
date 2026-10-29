using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class InfrastructureRegistrationTests
{
    [Fact]
    public void RegistrationIsScopedIdempotentAndReturnsTheOriginalCollection()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.AddSharpQrs());
        var original = services.ToArray();
        Assert.Same(services, services.AddSharpQrs());
        Assert.Equal(original, services.ToArray());
        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, d => d.ServiceType == typeof(ICommandDispatcher)).Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, Assert.Single(services, d => d.ServiceType == typeof(IQueryDispatcher)).Lifetime);
    }

    [Fact]
    public async Task SelectedHandlersWorkWithoutExplicitInfrastructureSetup()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.AddSharpQrsCommand<EchoCommand, EchoResult, EchoCommandHandler>());
        Assert.Same(services, services.AddSharpQrsQuery<EchoQuery, EchoResult, EchoQueryHandler>());
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        Assert.Equal(new EchoResult("command"), await scope.ServiceProvider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new EchoCommand("command")));
        Assert.Equal(new EchoResult("query"), await scope.ServiceProvider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new EchoQuery("query")));
    }
}
