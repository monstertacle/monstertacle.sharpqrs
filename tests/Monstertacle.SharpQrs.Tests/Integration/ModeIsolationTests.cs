using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ModeIsolationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SyncSelectionsCoexistWithExternalAsyncInfrastructure(bool query, bool contract)
    {
        var services = new ServiceCollection();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        if (query)
        {
            if (contract)
            {
                services.AddScoped<ISyncQueryHandler<ProbeRequest<Guid>, Guid>, SyncScopeHandler>();
                services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid>();
            }
            else services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        }
        else
        {
            if (contract)
            {
                services.AddScoped<ISyncCommandHandler<ProbeRequest<Guid>, Guid>, SyncScopeHandler>();
                services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid>();
            }
            else services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        }
        using var provider = services.BuildValidated();
        using var scope = provider.CreateScope();
        var result = query
            ? scope.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(new ProbeRequest<Guid>())
            : scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>().Dispatch(new ProbeRequest<Guid>());
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IQueryDispatcher>());
    }
}
