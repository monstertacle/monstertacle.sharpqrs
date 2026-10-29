using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class CommandDispatchTests
{
    [Fact]
    public async Task DispatchReturnsTheResultAndInvokesExactlyOnce()
    {
        var dispatcher = new CommandDispatcher();
        var handler = new EchoCommandHandler();
        dispatcher.Register(handler);
        Assert.Equal(new EchoResult("hello"), await dispatcher.DispatchAsync(new EchoCommand("hello")));
        Assert.Equal(1, handler.Calls);
    }
}
