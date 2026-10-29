using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class WrongModeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WrongModeNeverInvokesTheOtherHandler(bool query)
    {
        var services = new ServiceCollection().AddSharpQrs().AddSharpQrsSynchronous();
        using var handler = new SyncProbeHandler<int>((_, _) => throw new InvalidOperationException("must not execute"));
        if (query)
        {
            services.AddSingleton<ISyncQueryHandler<ProbeRequest<int>, int>>(handler);
            services.AddSharpQrsSyncQuery<ProbeRequest<int>, int>();
        }
        else
        {
            services.AddSingleton<ISyncCommandHandler<ProbeRequest<int>, int>>(handler);
            services.AddSharpQrsSyncCommand<ProbeRequest<int>, int>();
        }
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        await Assert.ThrowsAsync<InvalidOperationException>(() => query
            ? scope.ServiceProvider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new ProbeRequest<int>())
            : scope.ServiceProvider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new ProbeRequest<int>()));
        Assert.Equal(0, handler.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AsyncMappingsAreNotVisibleToSynchronousDispatch(bool query)
    {
        var services = new ServiceCollection().AddSharpQrsSynchronous();
        if (query) services.AddSharpQrsQuery<ProbeRequest<int>, int, ProbeHandler<int>>();
        else services.AddSharpQrsCommand<ProbeRequest<int>, int, ProbeHandler<int>>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        Assert.Throws<InvalidOperationException>(() => query
            ? scope.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(new ProbeRequest<int>())
            : scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>().Dispatch(new ProbeRequest<int>()));
    }
}
