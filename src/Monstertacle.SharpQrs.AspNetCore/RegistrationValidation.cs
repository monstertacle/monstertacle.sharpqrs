using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class RegistrationValidation
{
    internal static void Infrastructure(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var owned = services.Select(Instance).OfType<InfrastructureRegistration>().SingleOrDefault();
        var dispatchers = services.Where(d => d.ServiceType == typeof(ICommandDispatcher) || d.ServiceType == typeof(IQueryDispatcher));
        foreach (var descriptor in dispatchers)
        {
            if (owned is null || (descriptor != owned.Commands && descriptor != owned.Queries))
                throw new InvalidOperationException($"External dispatcher registration conflicts with {descriptor.ServiceType.FullName}.");
        }
    }

    internal static void Handler(IServiceCollection services, Type handler)
    {
        Infrastructure(services);
        if (handler.IsAbstract || handler.ContainsGenericParameters)
            throw new InvalidOperationException($"Handler {handler.FullName} must be concrete and closed.");
        var owned = services.Select(Instance).OfType<RegisteredHandlerType>().Any(r => r.HandlerType == handler);
        if (!owned && services.Any(d => d.ServiceType == handler))
            throw new InvalidOperationException($"External handler registration conflicts with {handler.FullName}.");
    }

    internal static void Mapping(IServiceCollection services, Type request, bool query)
    {
        var duplicate = query
            ? services.Select(Instance).OfType<QueryHandlerRegistration>().Any(r => r.RequestType == request)
            : services.Select(Instance).OfType<CommandHandlerRegistration>().Any(r => r.RequestType == request);
        if (duplicate)
            throw new InvalidOperationException($"A handler for {request.FullName} is already registered.");
    }

    private static object? Instance(ServiceDescriptor descriptor)
        => descriptor.IsKeyedService ? null : descriptor.ImplementationInstance;
}
