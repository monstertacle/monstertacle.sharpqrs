using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class ConcurrentDispatchTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OverlappingCallsRetainTheirOwnRequestsAndResults(bool query)
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler = new ProbeHandler<int>(async (request, _) =>
        {
            started.TrySetResult();
            await release.Task;
            return request.Id;
        });
        var probe = new DispatchProbe<int>(query, handler);
        var first = probe.DispatchAsync(new ProbeRequest<int>(11));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var second = probe.DispatchAsync(new ProbeRequest<int>(22));
        Assert.False(first.IsCompleted);
        Assert.False(second.IsCompleted);
        release.SetResult();
        var results = await Task.WhenAll(first, second);
        Assert.Collection(results, value => Assert.Equal(11, value), value => Assert.Equal(22, value));
        Assert.Equal(2, handler.Calls);
    }
}
