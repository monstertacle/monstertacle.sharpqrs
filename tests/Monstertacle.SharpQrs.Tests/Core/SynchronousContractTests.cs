using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class SynchronousContractTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryActualNullResultFails(bool query)
    {
        var dispatcher = new SyncDispatch(query);
        using var handler = new SyncProbeHandler<string?>((_, _) => null);
        dispatcher.Register(handler);
        Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(new ProbeRequest<string?>()));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ExceptionsAndCancellationKeepTheirIdentity(bool query, bool cancelled)
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        Exception error = cancelled ? new OperationCanceledException(source.Token) : new InvalidOperationException("original");
        var dispatcher = new SyncDispatch(query);
        using var handler = new SyncProbeHandler<int>((_, _) => throw error);
        dispatcher.Register(handler);
        Assert.Same(error, Record.Exception(() => dispatcher.Dispatch(new ProbeRequest<int>(), source.Token)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WrongResultContractFailsBeforeInvocation(bool query)
    {
        using var handler = new SyncMultiResultHandler();
        if (query)
        {
            var dispatcher = new SyncQueryDispatcher();
            dispatcher.Register<MultiResultRequest, string>(handler);
            Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch((IQuery<int>)new MultiResultRequest()));
        }
        else
        {
            var dispatcher = new SyncCommandDispatcher();
            dispatcher.Register<MultiResultRequest, string>(handler);
            Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch((ICommand<int>)new MultiResultRequest()));
        }
        Assert.Equal(0, handler.Calls);
    }
}
