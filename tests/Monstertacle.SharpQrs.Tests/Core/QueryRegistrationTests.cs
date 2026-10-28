using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class QueryRegistrationTests
{
    [Fact]
    public void NullHandlerIsRejectedExplicitly()
    {
        var dispatcher = new QueryDispatcher();
        Assert.Throws<ArgumentNullException>(() => dispatcher.Register<EchoQuery, EchoResult>(null!));
    }

    [Fact]
    public async Task DuplicateIsRejectedAndOriginalPreserved()
    {
        var dispatcher = new QueryDispatcher();
        var original = new EchoQueryHandler();
        var replacement = new EchoQueryHandler();
        dispatcher.Register(original);
        var exception = Assert.Throws<InvalidOperationException>(() => dispatcher.Register(replacement));
        Assert.Contains(typeof(EchoQuery).FullName!, exception.Message);
        await dispatcher.DispatchAsync(new EchoQuery("original"));
        Assert.Equal(1, original.Calls);
        Assert.Equal(0, replacement.Calls);
    }
}
