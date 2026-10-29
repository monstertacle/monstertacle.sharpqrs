using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class CompositionValidation
{
    internal static IEnumerable<T> Instances<T>(IServiceCollection services)
        => services.Where(d => !d.IsKeyedService).Select(d => d.ImplementationInstance).OfType<T>();

    internal static void Validate(IServiceCollection services)
    {
        foreach (var selected in Instances<SelectedHandlerService>(services))
            HandlerServiceSelection.Validate(services, selected.ServiceType);
        foreach (var owned in Instances<RegisteredHandlerType>(services))
        {
            var descriptors = services.Where(d => !d.IsKeyedService && d.ServiceType == owned.HandlerType).ToArray();
            if (descriptors.Length != 1 || descriptors[0] != owned.Descriptor)
                throw new InvalidOperationException($"Owned handler registration changed for {owned.HandlerType.FullName}.");
        }
        ValidateInfrastructure(services);
        Unique(Instances<CommandHandlerRegistration>(services).Select(r => r.RequestType));
        Unique(Instances<QueryHandlerRegistration>(services).Select(r => r.RequestType));
        Unique(Instances<SyncCommandHandlerRegistration>(services).Select(r => r.RequestType));
        Unique(Instances<SyncQueryHandlerRegistration>(services).Select(r => r.RequestType));
    }

    private static void ValidateInfrastructure(IServiceCollection services)
    {
        foreach (var owned in Instances<InfrastructureRegistration>(services))
        {
            RegistrationValidation.Infrastructure(services);
            RequireDescriptors(services, owned.Commands, owned.Queries);
        }
        foreach (var owned in Instances<SyncInfrastructureRegistration>(services))
        {
            SyncRegistrationValidation.Infrastructure(services);
            RequireDescriptors(services, owned.Commands, owned.Queries);
        }
    }

    private static void RequireDescriptors(IServiceCollection services, ServiceDescriptor commands, ServiceDescriptor queries)
    {
        if (!services.Contains(commands) || !services.Contains(queries))
            throw new InvalidOperationException("Owned dispatcher registration was removed or replaced.");
    }

    private static void Unique(IEnumerable<Type> requests)
    {
        var seen = new HashSet<Type>();
        foreach (var request in requests)
        {
            if (!seen.Add(request))
                throw new InvalidOperationException($"Multiple handler mappings exist for {request.FullName}.");
        }
    }
}
