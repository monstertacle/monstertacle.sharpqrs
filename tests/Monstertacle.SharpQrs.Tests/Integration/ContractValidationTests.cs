using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ContractValidationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrAmbiguousServicesFailWithoutChangingDescriptors(bool query)
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => Map(services, query));
        Assert.Empty(services);
        Add(services, query);
        Add(services, query);
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Map(services, query));
        Assert.Equal(before, services);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaterAmbiguityFailsBeforeActivation(bool query)
    {
        var services = new ServiceCollection();
        Add(services, query);
        Map(services, query);
        Add(services, query);
        Assert.Throws<InvalidOperationException>(() => services.BuildValidated());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DuplicateMappingPreservesDescriptors(bool query)
    {
        var services = new ServiceCollection();
        Add(services, query);
        Map(services, query);
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Map(services, query));
        Assert.Equal(before, services);
    }

    [Fact]
    public void NullCollectionsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => SharpQrsContractExtensions.AddSharpQrsCommand<EchoCommand, EchoResult>(null!));
        Assert.Throws<ArgumentNullException>(() => SharpQrsContractExtensions.AddSharpQrsQuery<EchoQuery, EchoResult>(null!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KeyedServicesNeverCountAsTheSelectedUnkeyedContract(bool query)
    {
        var services = new ServiceCollection();
        if (query) services.AddKeyedScoped<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>("other");
        else services.AddKeyedScoped<ICommandHandler<EchoCommand, EchoResult>, EchoCommandHandler>("other");
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Map(services, query));
        Assert.Equal(before, services);
        Add(services, query);
        Map(services, query);
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var type = query ? typeof(IQueryDispatcher) : typeof(ICommandDispatcher);
        Assert.NotNull(scope.ServiceProvider.GetRequiredService(type));
    }

    private static void Map(IServiceCollection services, bool query)
    {
        if (query) services.AddSharpQrsQuery<EchoQuery, EchoResult>();
        else services.AddSharpQrsCommand<EchoCommand, EchoResult>();
    }

    private static void Add(IServiceCollection services, bool query)
    {
        if (query) services.AddScoped<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>();
        else services.AddScoped<ICommandHandler<EchoCommand, EchoResult>, EchoCommandHandler>();
    }
}
