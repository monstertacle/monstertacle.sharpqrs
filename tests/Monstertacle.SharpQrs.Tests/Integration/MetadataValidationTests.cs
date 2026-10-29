using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class MetadataValidationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DuplicateMetadataFailsBeforeHandlerActivation(bool query)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddEcho(query);
        var metadataName = query ? "QueryHandlerRegistration" : "CommandHandlerRegistration";
        services.Add(Assert.Single(services, d => d.ServiceType.Name == metadataName));
        Assert.Throws<InvalidOperationException>(() => services.BuildValidated());
        using var legacy = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = legacy.CreateScope();
        var dispatcherType = query ? typeof(IQueryDispatcher) : typeof(ICommandDispatcher);
        Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService(dispatcherType));
    }
}
