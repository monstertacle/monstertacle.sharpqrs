using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class QueryDispatchTests
{
    [Fact]
    public async Task DispatchReturnsTheResultAndInvokesExactlyOnce()
    {
        var dispatcher = new QueryDispatcher();
        var handler = new EchoQueryHandler();
        dispatcher.Register(handler);
        Assert.Equal(new EchoResult("hello"), await dispatcher.DispatchAsync(new EchoQuery("hello")));
        Assert.Equal(1, handler.Calls);
    }
}
