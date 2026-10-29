using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class SynchronousScopeTests
{
    [Fact]
    public void ConcreteHandlerIsSharedAcrossMappingsAndDisposedOnce()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<object>("unrelated", new object());
        services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        Assert.Same(services, services.AddSharpQrsSynchronous());
        Assert.Single(services, d => d.ServiceType == typeof(SyncScopeHandler));
        using var provider = services.BuildValidated();
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ISyncCommandDispatcher>());
        var first = provider.CreateScope();
        var handler = first.ServiceProvider.GetRequiredService<SyncScopeHandler>();
        var command = first.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>();
        var query = first.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>();
        Assert.Equal(handler.Id, command.Dispatch(new ProbeRequest<Guid>()));
        Assert.Equal(handler.Id, query.Dispatch(new ProbeRequest<Guid>()));
        using (var second = provider.CreateScope())
            Assert.NotEqual(handler.Id, second.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(new ProbeRequest<Guid>()));
        Assert.Equal(0, handler.Disposals);
        first.Dispose();
        Assert.Equal(1, handler.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InterfaceHandlersAreLazyAndOwnedByTheirScope(bool query)
    {
        var services = new ServiceCollection();
        var activations = 0;
        if (query)
        {
            services.AddScoped<ISyncQueryHandler<ProbeRequest<Guid>, Guid>>(_ => { activations++; return new SyncScopeHandler(); });
            services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid>();
        }
        else
        {
            services.AddScoped<ISyncCommandHandler<ProbeRequest<Guid>, Guid>>(_ => { activations++; return new SyncScopeHandler(); });
            services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid>();
        }
        using var provider = services.BuildValidated();
        var scope = provider.CreateScope();
        var commands = scope.ServiceProvider.GetRequiredService<ISyncCommandDispatcher>();
        var queries = scope.ServiceProvider.GetRequiredService<ISyncQueryDispatcher>();
        Assert.Equal(0, activations);
        var result = query ? queries.Dispatch(new ProbeRequest<Guid>()) : commands.Dispatch(new ProbeRequest<Guid>());
        var type = query ? typeof(ISyncQueryHandler<ProbeRequest<Guid>, Guid>) : typeof(ISyncCommandHandler<ProbeRequest<Guid>, Guid>);
        var handler = Assert.IsType<SyncScopeHandler>(scope.ServiceProvider.GetRequiredService(type));
        Assert.Equal(handler.Id, result);
        Assert.Equal(1, activations);
        scope.Dispose();
        Assert.Equal(1, handler.Disposals);
    }
}
