using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class RegistrationConflictTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DuplicateMappingFailsBeforeMutationAndPreservesOriginal(bool query)
    {
        var services = new ServiceCollection();
        services.AddEcho(query);
        var original = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.AddEcho(query));
        Assert.Equal(original, services.ToArray());
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        Assert.Equal(new EchoResult("echo"), await scope.ServiceProvider.DispatchEcho(query));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExternalConcreteHandlerIsRejectedAtomically(bool query)
    {
        var services = new ServiceCollection();
        if (query) services.AddScoped<EchoQueryHandler>();
        else services.AddSingleton<EchoCommandHandler>();
        var original = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.AddEcho(query));
        Assert.Equal(original, services.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExternalDispatcherIsRejectedAtomically(bool query)
    {
        var services = new ServiceCollection();
        if (query) services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        else services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        var original = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.AddEcho(query));
        Assert.Equal(original, services.ToArray());
        Assert.Throws<InvalidOperationException>(() => services.AddSharpQrs());
        Assert.Equal(original, services.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbstractHandlerIsRejectedAtomically(bool query)
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() =>
        {
            if (query) services.AddSharpQrsQuery<EchoQuery, EchoResult, AbstractEchoHandler>();
            else services.AddSharpQrsCommand<EchoCommand, EchoResult, AbstractEchoHandler>();
        });
        Assert.Empty(services);
    }

    [Fact]
    public void NullCollectionsAreRejectedExplicitly()
    {
        Assert.Throws<ArgumentNullException>(() => SharpQrsServiceCollectionExtensions.AddSharpQrs(null!));
        Assert.Throws<ArgumentNullException>(() => SharpQrsServiceCollectionExtensions.AddSharpQrsCommand<EchoCommand, EchoResult, EchoCommandHandler>(null!));
        Assert.Throws<ArgumentNullException>(() => SharpQrsServiceCollectionExtensions.AddSharpQrsQuery<EchoQuery, EchoResult, EchoQueryHandler>(null!));
    }
}
