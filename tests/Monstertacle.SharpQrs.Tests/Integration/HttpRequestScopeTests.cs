using Monstertacle.SharpQrs.Tests.Fixtures.Http;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class HttpRequestScopeTests
{
    [Fact]
    public async Task ActualRequestsHaveSeparateScopesAndDisposeTheirServicesOnce()
    {
        await using var host = await HttpScopeHost.StartAsync();
        var first = (await host.Client.GetStringAsync("/probe")).Split(';');
        var second = (await host.Client.GetStringAsync("/probe")).Split(';');
        Assert.Equal(first[0], first[1]);
        Assert.Equal(second[0], second[1]);
        Assert.NotEqual(first[0], second[0]);
        Assert.Equal(2, host.Handlers.Count);
        foreach (var handler in host.Handlers)
        {
            await handler.State.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(1, handler.Disposals);
            Assert.Equal(1, handler.State.Disposals);
        }
    }
}
