using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class SynchronousServiceContractTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScopedDispatchPreservesRequestAndCancelledTokenAndOriginalError(bool query)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var request = new ProbeRequest<int>();
        var error = new OperationCanceledException(cancellation.Token);
        var calls = 0;
        var services = new ServiceCollection();
        var handler = new SyncProbeHandler<int>((actual, token) =>
        {
            Assert.Same(request, actual);
            Assert.Equal(cancellation.Token, token);
            calls++;
            throw error;
        });
        if (query) services.AddScoped<ISyncQueryHandler<ProbeRequest<int>, int>>(_ => handler).AddSharpQrsSyncQuery<ProbeRequest<int>, int>();
        else services.AddScoped<ISyncCommandHandler<ProbeRequest<int>, int>>(_ => handler).AddSharpQrsSyncCommand<ProbeRequest<int>, int>();
        using var provider = services.BuildValidated();
        using (var scope = provider.CreateScope())
        {
            var actual = Record.Exception(() => query
                ? scope.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(request, cancellation.Token)
                : scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>().Dispatch(request, cancellation.Token));
            Assert.Same(error, actual);
            Assert.Equal(1, calls);
        }
        Assert.True(handler.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SynchronousHandlersMayOwnAsynchronouslyDisposedResources(bool query)
    {
        var services = new ServiceCollection();
        if (query) services.AddSharpQrsSyncQuery<ProbeRequest<int>, int, AsyncSyncHandler>();
        else services.AddSharpQrsSyncCommand<ProbeRequest<int>, int, AsyncSyncHandler>();
        await using var provider = services.BuildValidated();
        var scope = provider.CreateAsyncScope();
        var result = query
            ? scope.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(new ProbeRequest<int>())
            : scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>().Dispatch(new ProbeRequest<int>());
        Assert.Equal(42, result);
        var handler = scope.ServiceProvider.GetRequiredService<AsyncSyncHandler>();
        Assert.Equal(0, handler.Disposals);
        await scope.DisposeAsync();
        Assert.Equal(1, handler.Disposals);
    }
}
