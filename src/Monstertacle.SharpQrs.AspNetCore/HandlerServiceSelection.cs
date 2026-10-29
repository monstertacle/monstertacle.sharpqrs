using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal static class HandlerServiceSelection
{
    internal static void Validate<THandler>(IServiceCollection services)
        => Validate(services, typeof(THandler));

    internal static void Validate(IServiceCollection services, Type handlerType)
    {
        ArgumentNullException.ThrowIfNull(services);
        var count = services.Count(d => !d.IsKeyedService && d.ServiceType == handlerType);
        if (count != 1)
            throw new InvalidOperationException($"Expected exactly one unkeyed handler service for {handlerType.FullName}; found {count}.");
    }

    internal static void RequirePrepared<THandler>(IServiceProvider provider)
    {
        var prepared = provider.GetService(typeof(PreparedComposition)) as PreparedComposition;
        if (prepared is null || !prepared.Contains(typeof(THandler)))
            throw new InvalidOperationException("Interface handler mappings require PrepareSharpQrs() or SharpQrsServiceProviderFactory before provider construction.");
    }
}
