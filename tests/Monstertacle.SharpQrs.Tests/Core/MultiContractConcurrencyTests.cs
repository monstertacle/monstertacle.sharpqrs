using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class MultiContractConcurrencyTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DifferentRequestAndResultTypesRetainTheirAssociations(bool query)
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var number = new ProbeHandler<int>(async (request, _) => { await release.Task; return request.Id; });
        using var text = new ProbeHandler<string>(async (_, _) => { await release.Task; return "text"; });
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        commands.Register(number);
        commands.Register(text);
        queries.Register(number);
        queries.Register(text);
        var first = Dispatch<int>(query, commands, queries, new ProbeRequest<int>(42));
        var second = Dispatch<string>(query, commands, queries, new ProbeRequest<string>());
        Assert.False(first.IsCompleted);
        Assert.False(second.IsCompleted);
        release.SetResult();
        Assert.Equal(42, await first);
        Assert.Equal("text", await second);
        Assert.Equal(1, number.Calls);
        Assert.Equal(1, text.Calls);
    }

    private static Task<TResult> Dispatch<TResult>(bool query, CommandDispatcher commands, QueryDispatcher queries, ProbeRequest<TResult> request)
        => query ? queries.DispatchAsync(request) : commands.DispatchAsync(request);
}
