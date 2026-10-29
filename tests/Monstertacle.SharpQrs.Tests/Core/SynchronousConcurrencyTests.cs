using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class SynchronousConcurrencyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConcurrentDispatchUsesStableMappingsAndNoSharedResultState(bool query)
    {
        var dispatcher = new SyncDispatch(query);
        using var handler = new SyncProbeHandler<int>((request, _) => request.Id * 2);
        dispatcher.Register(handler);
        var results = await Task.WhenAll(Enumerable.Range(0, 32)
            .Select(id => Task.Run(() => dispatcher.Dispatch(new ProbeRequest<int>(id)))));
        Assert.Equal(Enumerable.Range(0, 32).Select(id => id * 2), results);
        Assert.Equal(32, handler.Calls);
        Assert.Throws<InvalidOperationException>(() => new SyncDispatch(query).Dispatch(new ProbeRequest<int>()));
    }
}
