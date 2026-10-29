using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class CommandRegistrationTests
{
    [Fact]
    public void NullHandlerIsRejectedExplicitly()
    {
        var dispatcher = new CommandDispatcher();
        Assert.Throws<ArgumentNullException>(() => dispatcher.Register<EchoCommand, EchoResult>(null!));
    }

    [Fact]
    public async Task DuplicateIsRejectedAndOriginalPreserved()
    {
        var dispatcher = new CommandDispatcher();
        var original = new EchoCommandHandler();
        var replacement = new EchoCommandHandler();
        dispatcher.Register(original);
        var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.Register(replacement));
        Assert.Contains(typeof(EchoCommand).FullName!, exception.Message);
        await dispatcher.DispatchAsync(new EchoCommand("original"));
        Assert.Equal(1, original.Calls);
        Assert.Equal(0, replacement.Calls);
    }
}
