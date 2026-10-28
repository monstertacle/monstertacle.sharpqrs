using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class ResultIdentityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task WrongResultContractFailsBeforeInvocation(bool query, bool integrated)
    {
        var services = new ServiceCollection();
        services.AddSharpQrsCommand<MultiResultRequest, int, MultiResultHandler>();
        services.AddSharpQrsQuery<MultiResultRequest, int, MultiResultHandler>();
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var handler = integrated ? scope.ServiceProvider.GetRequiredService<MultiResultHandler>() : new MultiResultHandler();
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        commands.Register<MultiResultRequest, int>(handler);
        queries.Register<MultiResultRequest, int>(handler);
        ICommandDispatcher commandDispatcher = integrated ? scope.ServiceProvider.GetRequiredService<ICommandDispatcher>() : commands;
        IQueryDispatcher queryDispatcher = integrated ? scope.ServiceProvider.GetRequiredService<IQueryDispatcher>() : queries;
        Task<string> Dispatch() => query
            ? queryDispatcher.DispatchAsync<string>(new MultiResultRequest())
            : commandDispatcher.DispatchAsync<string>(new MultiResultRequest());
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(Dispatch);
        Assert.Contains(typeof(MultiResultRequest).FullName!, exception.Message);
        Assert.Equal(0, handler.Calls);
    }
}
