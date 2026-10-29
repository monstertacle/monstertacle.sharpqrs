using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

/// <summary>Prepares a final, independent collection for checked container construction.</summary>
public static class SharpQrsPreparationExtensions
{
    /// <summary>Validates mappings without activation and returns a read-only copy. The input is unchanged.</summary>
    /// <param name="services">The final configured collection, before building its provider.</param>
    /// <returns>An independent, read-only collection. Build from this returned collection.</returns>
    /// <exception cref="ArgumentNullException">The input is null.</exception>
    /// <exception cref="InvalidOperationException">Mappings or selected services conflict or are missing.</exception>
    /// <remarks>Finish configuration before preparation; concurrent mutation is unsupported.
    /// Prepare again after copying or customising descriptors. Do not copy prepared evidence and bypass preparation.</remarks>
    public static IServiceCollection PrepareSharpQrs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        CompositionValidation.Validate(services);
        var prepared = new ServiceCollection();
        foreach (var descriptor in services.Where(d => d.ServiceType != typeof(PreparedComposition)))
            ((IServiceCollection)prepared).Add(descriptor);
        var selected = CompositionValidation.Instances<SelectedHandlerService>(services).Select(r => r.ServiceType);
        prepared.AddSingleton(new PreparedComposition(selected));
        prepared.MakeReadOnly();
        return prepared;
    }
}
