using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class DispatcherOwnershipTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandlersRemainCallerOwnedAndDispatchersAreIndependent(bool query)
    {
        using var first = new ProbeHandler<int>((_, _) => Task.FromResult(1));
        using var second = new ProbeHandler<int>((_, _) => Task.FromResult(2));
        var left = new DispatchProbe<int>(query, first);
        var right = new DispatchProbe<int>(query, second);
        Assert.Equal(1, await left.DispatchAsync(new ProbeRequest<int>()));
        Assert.Equal(2, await right.DispatchAsync(new ProbeRequest<int>()));
        Assert.False(first.Disposed);
        Assert.False(second.Disposed);
        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(CommandDispatcher)));
        Assert.False(typeof(IAsyncDisposable).IsAssignableFrom(typeof(QueryDispatcher)));
    }

    [Fact]
    public async Task CategoriesHaveIndependentRegistrations()
    {
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        using var command = new ProbeHandler<int>((_, _) => Task.FromResult(1));
        using var query = new ProbeHandler<int>((_, _) => Task.FromResult(2));
        commands.Register(command);
        queries.Register(query);
        var request = new ProbeRequest<int>();
        Assert.Equal(1, await commands.DispatchAsync(request));
        Assert.Equal(2, await queries.DispatchAsync(request));
    }
}
