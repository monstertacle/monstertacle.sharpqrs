using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class ScopedDispatcherFactory
{
    internal static ICommandDispatcher CreateCommand(IServiceProvider provider)
    {
        var registrations = provider.GetServices<CommandHandlerRegistration>().ToArray();
        ValidateRequests(registrations.Select(registration => registration.RequestType));
        var dispatcher = new CommandDispatcher();
        foreach (var registration in registrations)
            registration.Install(dispatcher, provider);
        return dispatcher;
    }

    internal static IQueryDispatcher CreateQuery(IServiceProvider provider)
    {
        var registrations = provider.GetServices<QueryHandlerRegistration>().ToArray();
        ValidateRequests(registrations.Select(registration => registration.RequestType));
        var dispatcher = new QueryDispatcher();
        foreach (var registration in registrations)
            registration.Install(dispatcher, provider);
        return dispatcher;
    }
    private static void ValidateRequests(IEnumerable<Type> requests)
    {
        var seen = new HashSet<Type>();
        foreach (var request in requests)
        {
            if (!seen.Add(request))
                throw new InvalidOperationException($"Multiple handler mappings exist for {request.FullName}.");
        }
    }
}
