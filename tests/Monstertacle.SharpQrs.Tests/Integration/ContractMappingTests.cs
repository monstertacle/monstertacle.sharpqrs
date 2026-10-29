using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ContractMappingTests
{
    [Fact]
    public async Task ExistingInterfaceServicesAreSelectedLazily()
    {
        var calls = 0;
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<EchoCommand, EchoResult>>(_ =>
        {
            calls++;
            return new EchoCommandHandler();
        });
        services.AddScoped<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>();
        Assert.Same(services, services.AddSharpQrsCommand<EchoCommand, EchoResult>());
        services.AddSharpQrsQuery<EchoQuery, EchoResult>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var commands = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var queries = scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
        Assert.Equal(0, calls);
        Assert.Equal(new EchoResult("command"), await commands.DispatchAsync(new EchoCommand("command")));
        Assert.Equal(new EchoResult("query"), await queries.DispatchAsync(new EchoQuery("query")));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task StandaloneSetupIsAvailableThroughSeparateContracts()
    {
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        Assert.IsAssignableFrom<ICommandRegistration>(commands).Register(new EchoCommandHandler());
        Assert.IsAssignableFrom<IQueryRegistration>(queries).Register(new EchoQueryHandler());
        Assert.Equal(new EchoResult("c"), await commands.DispatchAsync(new EchoCommand("c")));
        Assert.Equal(new EchoResult("q"), await queries.DispatchAsync(new EchoQuery("q")));
    }
}
