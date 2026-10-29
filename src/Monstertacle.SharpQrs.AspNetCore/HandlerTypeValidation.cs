using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class HandlerTypeValidation
{
    internal static void Validate(IServiceCollection services, Type handler)
    {
        if (handler.IsAbstract || handler.ContainsGenericParameters)
            throw new InvalidOperationException($"Handler {handler.FullName} must be concrete and closed.");
        var owned = services.Where(d => !d.IsKeyedService).Select(d => d.ImplementationInstance)
            .OfType<RegisteredHandlerType>().Any(r => r.HandlerType == handler);
        if (!owned && services.Any(d => d.ServiceType == handler))
            throw new InvalidOperationException($"External handler registration conflicts with {handler.FullName}.");
    }
}
