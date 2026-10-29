using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ContractLifetimeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InterfaceDecoratorParticipatesAndScopeOwnsDisposal(bool query)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => new ProbeHandler<int>((_, _) => Task.FromResult(41)));
        if (query)
        {
            services.AddScoped<IQueryHandler<ProbeRequest<int>, int>>(p => new DecoratedProbeHandler(p.GetRequiredService<ProbeHandler<int>>()));
            services.AddSharpQrsQuery<ProbeRequest<int>, int>();
        }
        else
        {
            services.AddScoped<ICommandHandler<ProbeRequest<int>, int>>(p => new DecoratedProbeHandler(p.GetRequiredService<ProbeHandler<int>>()));
            services.AddSharpQrsCommand<ProbeRequest<int>, int>();
        }
        await using var provider = services.BuildValidated();
        var scope = provider.CreateAsyncScope();
        var type = query ? typeof(IQueryHandler<ProbeRequest<int>, int>) : typeof(ICommandHandler<ProbeRequest<int>, int>);
        var handler = Assert.IsType<DecoratedProbeHandler>(scope.ServiceProvider.GetRequiredService(type));
        var inner = scope.ServiceProvider.GetRequiredService<ProbeHandler<int>>();
        var result = query
            ? await scope.ServiceProvider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new ProbeRequest<int>())
            : await scope.ServiceProvider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new ProbeRequest<int>());
        Assert.Equal(42, result);
        Assert.Equal(1, handler.Calls);
        Assert.Equal(1, inner.Calls);
        await scope.DisposeAsync();
        Assert.Equal(1, handler.Disposals);
        Assert.True(inner.Disposed);
    }

    [Fact]
    public async Task TransientInterfaceRegistrationKeepsItsDeclaredLifetime()
    {
        var count = 0;
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<ProbeRequest<int>, int>>(_ => new ProbeHandler<int>((_, _) => Task.FromResult(++count)));
        services.AddSharpQrsCommand<ProbeRequest<int>, int>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        Assert.Equal(1, await dispatcher.DispatchAsync(new ProbeRequest<int>()));
        Assert.Equal(2, await dispatcher.DispatchAsync(new ProbeRequest<int>()));
    }
}
