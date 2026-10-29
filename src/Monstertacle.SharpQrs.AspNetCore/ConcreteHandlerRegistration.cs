using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class ConcreteHandlerRegistration
{
    internal static void Validate(IServiceCollection services, Type handler)
    {
        if (handler.IsAbstract || handler.ContainsGenericParameters)
            throw new InvalidOperationException($"Handler {handler.FullName} must be concrete and closed.");
        var owned = services.Where(d => !d.IsKeyedService).Select(d => d.ImplementationInstance)
            .OfType<RegisteredHandlerType>().Any(r => r.HandlerType == handler);
        if (!owned && services.Any(d => !d.IsKeyedService && d.ServiceType == handler))
            throw new InvalidOperationException($"External handler registration conflicts with {handler.FullName}.");
    }

    internal static void EnsureScoped<THandler>(IServiceCollection services) where THandler : class
    {
        if (!services.Any(d => !d.IsKeyedService && d.ServiceType == typeof(THandler)))
        {
            var descriptor = ServiceDescriptor.Scoped<THandler, THandler>();
            services.Add(descriptor);
            services.AddSingleton(new RegisteredHandlerType(typeof(THandler), descriptor));
        }
    }
}
