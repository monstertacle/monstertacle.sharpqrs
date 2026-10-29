using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class SyncScopedDispatcherFactory
{
    internal static ISyncCommandDispatcher CreateCommand(IServiceProvider provider)
    {
        var dispatcher = new SyncCommandDispatcher();
        foreach (var registration in provider.GetServices<SyncCommandHandlerRegistration>())
            registration.Install(dispatcher, provider);
        return dispatcher;
    }

    internal static ISyncQueryDispatcher CreateQuery(IServiceProvider provider)
    {
        var dispatcher = new SyncQueryDispatcher();
        foreach (var registration in provider.GetServices<SyncQueryHandlerRegistration>())
            registration.Install(dispatcher, provider);
        return dispatcher;
    }
}
