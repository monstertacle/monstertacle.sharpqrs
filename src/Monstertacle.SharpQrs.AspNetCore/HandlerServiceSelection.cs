using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class HandlerServiceSelection
{
    internal static void Validate<THandler>(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var count = services.Count(d => !d.IsKeyedService && d.ServiceType == typeof(THandler));
        if (count != 1)
            throw new InvalidOperationException($"Expected exactly one unkeyed handler service for {typeof(THandler).FullName}; found {count}.");
    }
}
