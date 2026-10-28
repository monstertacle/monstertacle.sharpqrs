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
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        Assert.Throws<InvalidOperationException>(() =>
        {
            if (query) scope.ServiceProvider.GetRequiredService<IQueryDispatcher>();
            else scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        });
    }
}
