using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class SyncRegistrationValidation
{
    internal static void Infrastructure(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var owned = services.Select(Instance).OfType<SyncInfrastructureRegistration>().SingleOrDefault();
        var dispatchers = services.Where(d => d.ServiceType == typeof(ISyncCommandDispatcher) || d.ServiceType == typeof(ISyncQueryDispatcher));
        foreach (var descriptor in dispatchers)
        {
            if (owned is null || (descriptor != owned.Commands && descriptor != owned.Queries))
                throw new InvalidOperationException($"External synchronous dispatcher registration conflicts with {descriptor.ServiceType.FullName}.");
        }
    }

    internal static void Mapping(IServiceCollection services, Type request, bool query)
    {
        var duplicate = query
            ? services.Select(Instance).OfType<SyncQueryHandlerRegistration>().Any(r => r.RequestType == request)
            : services.Select(Instance).OfType<SyncCommandHandlerRegistration>().Any(r => r.RequestType == request);
        if (duplicate)
            throw new InvalidOperationException($"A synchronous handler for {request.FullName} is already registered.");
    }

    internal static void Concrete(IServiceCollection services, Type handler)
    {
        SyncRegistrationValidation.Infrastructure(services);
        ConcreteHandlerRegistration.Validate(services, handler);
    }

    private static object? Instance(ServiceDescriptor descriptor)
        => descriptor.IsKeyedService ? null : descriptor.ImplementationInstance;
}
