using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ConcretePreparationTests
{
    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)] [InlineData(3, false)]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)] [InlineData(3, true)]
    public void LaterConcreteAdditionOrReplacementIsRejectedWithoutMutation(int mode, bool replace)
    {
        var services = new ServiceCollection();
        ContractSetup.SelectConcrete(services, mode);
        if (replace) services.Remove(Assert.Single(services, d => d.ServiceType == typeof(CompositionHandler)));
        services.AddScoped<CompositionHandler>();
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.PrepareSharpQrs());
        Assert.Equal(before, services);
    }

    [Fact]
    public async Task KeyedConcreteServicesDoNotConflictAndSharedOwnershipRemainsUnique()
    {
        var services = new ServiceCollection();
        services.AddKeyedScoped<CompositionHandler>("unrelated");
        for (var mode = 0; mode < 4; mode++) ContractSetup.SelectConcrete(services, mode);
        using var provider = services.PrepareSharpQrs().BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var scope = provider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CompositionHandler>();
        for (var mode = 0; mode < 4; mode++) Assert.Equal(42, await ContractDispatch.Execute(scope.ServiceProvider, mode));
        scope.Dispose();
        Assert.Equal(1, handler.Disposals);
    }

    [Fact]
    public void NullAndConflictingInfrastructureFailPreparation()
    {
        Assert.Throws<ArgumentNullException>(() => SharpQrsPreparationExtensions.PrepareSharpQrs(null!));
        var services = new ServiceCollection().AddSharpQrs();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        Assert.Throws<InvalidOperationException>(() => services.PrepareSharpQrs());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void RemovedOwnedDispatcherIsRejected(int mode)
    {
        var services = new ServiceCollection();
        ContractSetup.SelectConcrete(services, mode);
        var dispatcherType = mode switch
        {
            0 => typeof(ICommandDispatcher), 1 => typeof(IQueryDispatcher),
            2 => typeof(ISyncCommandDispatcher), _ => typeof(ISyncQueryDispatcher)
        };
        services.Remove(Assert.Single(services, d => d.ServiceType == dispatcherType));
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.PrepareSharpQrs());
        Assert.Equal(before, services);
    }
}
