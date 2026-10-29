using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class MixedModeTests
{
    [Fact]
    public async Task OneConcreteServiceCanBeSharedAcrossExecutionModes()
    {
        var services = new ServiceCollection();
        services.AddSharpQrsCommand<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        using var provider = services.BuildValidated();
        var scope = provider.CreateScope();
        var sync = scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>();
        var asyncDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var request = new ProbeRequest<Guid>();
        Assert.Equal(sync.Dispatch(request), await asyncDispatcher.DispatchAsync(request));
        var handler = scope.ServiceProvider.GetRequiredService<SyncScopeHandler>();
        scope.Dispose();
        Assert.Equal(1, handler.Disposals);
    }

}
