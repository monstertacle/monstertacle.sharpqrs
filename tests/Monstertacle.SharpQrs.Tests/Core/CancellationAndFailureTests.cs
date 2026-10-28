using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class CancellationAndFailureTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task AlreadyCancelledTokensReachHandlersUnchanged(bool query, bool integrated)
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, token) =>
        {
            Assert.Equal(source.Token, token);
            return Task.FromResult(17);
        });
        Assert.Equal(17, await environment.DispatchAsync(new ProbeRequest<int>(), source.Token));
        Assert.Equal(1, environment.Handler.Calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task CooperativeCancellationRetainsTheToken(bool query, bool integrated)
    {
        using var source = new CancellationTokenSource();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, token) =>
        {
            Assert.Equal(source.Token, token);
            started.SetResult();
            return completion.Task;
        });
        var pending = environment.DispatchAsync(new ProbeRequest<int>(), source.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        source.Cancel();
        completion.SetCanceled(source.Token);
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(source.Token, exception.CancellationToken);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task SynchronousFailuresPreserveExceptionIdentity(bool query, bool integrated)
    {
        var original = new InvalidOperationException("Application failure");
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, _) => throw original);
        Assert.Same(original, await Assert.ThrowsAsync<InvalidOperationException>(() => environment.DispatchAsync(new ProbeRequest<int>())));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task DelayedFailuresPreserveExceptionIdentity(bool query, bool integrated)
    {
        var original = new InvalidOperationException("Delayed application failure");
        var completion = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, _) => completion.Task);
        var pending = environment.DispatchAsync(new ProbeRequest<int>());
        Assert.False(pending.IsCompleted);
        completion.SetException(original);
        Assert.Same(original, await Assert.ThrowsAsync<InvalidOperationException>(() => pending));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ConcurrentConsumersRetainTheirOwnMappings(bool query, bool integrated)
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var first = new DispatchEnvironment<int>(query, integrated, async (request, _) =>
        {
            await release.Task;
            return request.Id + 10;
        });
        using var second = new DispatchEnvironment<int>(query, integrated, async (request, _) =>
        {
            await release.Task;
            return request.Id + 20;
        });
        var left = first.DispatchAsync(new ProbeRequest<int>(1));
        var right = second.DispatchAsync(new ProbeRequest<int>(2));
        Assert.False(left.IsCompleted);
        Assert.False(right.IsCompleted);
        release.SetResult();
        Assert.Equal(11, await left);
        Assert.Equal(22, await right);
    }
}
