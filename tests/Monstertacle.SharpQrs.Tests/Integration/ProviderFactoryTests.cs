using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ProviderFactoryTests
{
    [Fact]
    public void FactoryPreparesAfterBuilderConfigurationWithoutActivatingServices()
    {
        var inner = new DefaultServiceProviderFactory(new ServiceProviderOptions { ValidateScopes = true });
        var factory = new SharpQrsServiceProviderFactory(inner);
        var original = new ServiceCollection();
        ContractSetup.AddHandler(original, 0);
        ContractSetup.Select(original, 0);
        var builder = factory.CreateBuilder(original);
        Assert.Same(original, builder);
        ContractSetup.AddHandler(builder, 0);
        Assert.Throws<InvalidOperationException>(() => factory.CreateServiceProvider(builder));
        Assert.Equal(2, builder.Count(d => d.ServiceType == typeof(ICommandHandler<ProbeRequest<int>, int>)));
    }

    [Fact]
    public async Task FactoryPreservesOptionsAndUsesAnIndependentComposition()
    {
        var factory = new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory(
            new ServiceProviderOptions { ValidateScopes = true }));
        var services = new ServiceCollection();
        ContractSetup.AddHandler(services, 0);
        ContractSetup.Select(services, 0);
        using var provider = (ServiceProvider)factory.CreateServiceProvider(services);
        services.Clear();
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ICommandDispatcher>());
        await using var scope = provider.CreateAsyncScope();
        Assert.Equal(42, await ContractDispatch.Execute(scope.ServiceProvider, 0));
        Assert.Empty(services);
    }

    [Fact]
    public void FactoryRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new SharpQrsServiceProviderFactory(null!));
        var factory = new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory());
        Assert.Throws<ArgumentNullException>(() => factory.CreateBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => factory.CreateServiceProvider(null!));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public async Task GenericHostValidatesTheFinalContainerConfiguration(int mode)
    {
        using var host = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory(
                new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true })))
            .ConfigureServices(services => { ContractSetup.AddHandler(services, mode); ContractSetup.Select(services, mode); })
            .Build();
        await using var scope = host.Services.CreateAsyncScope();
        Assert.Equal(42, await ContractDispatch.Execute(scope.ServiceProvider, mode));
    }

    [Fact]
    public void HostContainerCallbacksCannotAddSilentAmbiguity()
    {
        var activations = 0;
        var builder = Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory()))
            .ConfigureServices(services => { ContractSetup.AddHandler(services, 0, activated: () => activations++); ContractSetup.Select(services, 0); })
            .ConfigureContainer<IServiceCollection>((_, services) => ContractSetup.AddHandler(services, 0));
        Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Equal(0, activations);
    }
}
