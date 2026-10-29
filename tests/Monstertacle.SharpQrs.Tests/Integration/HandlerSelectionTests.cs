using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class HandlerSelectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnselectedInterfaceRegistrationsAreNotDiscovered(bool query)
    {
        var services = new ServiceCollection();
        services.AddSharpQrs();
        services.AddSingleton<ICommandHandler<EchoCommand, EchoResult>, EchoCommandHandler>();
        services.AddSingleton<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.DispatchEcho(query));
        Assert.Contains(query ? typeof(EchoQuery).FullName! : typeof(EchoCommand).FullName!, error.Message);
        Assert.Null(scope.ServiceProvider.GetService<EchoCommandHandler>());
        Assert.Null(scope.ServiceProvider.GetService<EchoQueryHandler>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnrelatedKeyedServicesDoNotAffectSelection(bool query)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<object>("unrelated", new object());
        services.AddEcho(query);
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        Assert.Equal(new EchoResult("echo"), await scope.ServiceProvider.DispatchEcho(query));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ASecondHandlerForTheSameRequestIsRejectedAtomically(bool query)
    {
        var services = new ServiceCollection();
        services.AddEcho(query);
        var original = services.ToArray();
        Assert.Throws<InvalidOperationException>(() =>
        {
            if (query) services.AddSharpQrsQuery<EchoQuery, EchoResult, AlternativeEchoHandler>();
            else services.AddSharpQrsCommand<EchoCommand, EchoResult, AlternativeEchoHandler>();
        });
        Assert.Equal(original, services.ToArray());
    }
}
