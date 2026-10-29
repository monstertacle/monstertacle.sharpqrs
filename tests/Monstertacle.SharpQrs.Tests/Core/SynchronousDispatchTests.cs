using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class SynchronousDispatchTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DirectDispatchReturnsOriginalResultAndToken(bool query)
    {
        var dispatcher = new SyncDispatch(query);
        var request = new ProbeRequest<object>();
        var result = new object();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var handler = new SyncProbeHandler<object>((actual, token) =>
        {
            Assert.Same(request, actual);
            Assert.Equal(cancellation.Token, token);
            return result;
        });
        dispatcher.Register(handler);
        Assert.Same(result, dispatcher.Dispatch(request, cancellation.Token));
        Assert.Equal(1, handler.Calls);
        Assert.False(handler.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DuplicateRegistrationKeepsTheOriginalMapping(bool query)
    {
        var dispatcher = new SyncDispatch(query);
        using var original = new SyncProbeHandler<int>((_, _) => 1);
        using var replacement = new SyncProbeHandler<int>((_, _) => 2);
        dispatcher.Register(original);
        Assert.Throws<InvalidOperationException>(() => dispatcher.Register(replacement));
        Assert.Equal(1, dispatcher.Dispatch(new ProbeRequest<int>()));
        Assert.Equal(1, original.Calls);
        Assert.Equal(0, replacement.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingMappingsAndNullRequestsAndHandlersFailExplicitly(bool query)
    {
        var dispatcher = new SyncDispatch(query);
        Assert.Throws<ArgumentNullException>(() => dispatcher.Register<int>(null!));
        Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch<int>(null!));
        Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(new ProbeRequest<int>()));
    }
}
