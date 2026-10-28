using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Monstertacle.SharpQrs.Tests.Fixtures.Scopes;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ScopedResolutionTests
{
    [Fact]
    public async Task OneConcreteHandlerIsReusedAcrossContractsAndDisposedOnce()
    {
        var services = new ServiceCollection().AddScopedProbe();
        Assert.Single(services, d => d.ServiceType == typeof(ScopedHandler));
        using var provider = services.BuildValidated();
        var firstScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<ScopedHandler>();
        Assert.Equal(first.State.Id, await firstScope.ServiceProvider.DispatchScope(false));
        Assert.Equal(first.State.Id, await firstScope.ServiceProvider.DispatchScope(true));
        using (var secondScope = provider.CreateScope())
        {
            Assert.NotEqual(first.State.Id, await secondScope.ServiceProvider.DispatchScope(false));
            Assert.NotSame(first, secondScope.ServiceProvider.GetRequiredService<ScopedHandler>());
        }
        Assert.Equal(0, first.Disposals);
        firstScope.Dispose();
        Assert.Equal(1, first.Disposals);
        Assert.Equal(1, first.State.Disposals);
    }

    [Fact]
    public async Task AsyncOnlyMultiContractHandlerIsDisposedOnce()
    {
        var services = new ServiceCollection();
        services.AddSharpQrsCommand<ScopeCommand, Guid, AsyncScopedHandler>();
        services.AddSharpQrsQuery<ScopeQuery, Guid, AsyncScopedHandler>();
        await using var provider = services.BuildValidated();
        var scope = provider.CreateAsyncScope();
        Assert.Equal(await scope.ServiceProvider.DispatchScope(false), await scope.ServiceProvider.DispatchScope(true));
        var handler = scope.ServiceProvider.GetRequiredService<AsyncScopedHandler>();
        Assert.Equal(0, handler.Disposals);
        await scope.DisposeAsync();
        Assert.Equal(1, handler.Disposals);
    }

    [Fact]
    public void RootResolutionFailsWithScopeValidation()
    {
        using var provider = new ServiceCollection().AddScopedProbe().BuildValidated();
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ICommandDispatcher>());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IQueryDispatcher>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnrelatedHandlersRemainLazyAndMissingDependenciesPropagate(bool query)
    {
        var services = new ServiceCollection();
        services.AddEcho(query);
        services.AddSharpQrsCommand<ScopeCommand, Guid, ScopedHandler>();
        services.AddSharpQrsQuery<ScopeQuery, Guid, ScopedHandler>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        Assert.Equal(new EchoResult("echo"), await scope.ServiceProvider.DispatchEcho(query));
        await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.DispatchScope(query));
    }

    [Fact]
    public async Task ConcurrentDispatchesShareOnlyTheCurrentScope()
    {
        using var provider = new ServiceCollection().AddScopedProbe().BuildValidated();
        using var scope = provider.CreateScope();
        var tasks = Enumerable.Range(0, 20).Select(i => scope.ServiceProvider.DispatchScope(i % 2 == 0));
        var results = await Task.WhenAll(tasks);
        Assert.All(results, result => Assert.Equal(results[0], result));
    }
}
