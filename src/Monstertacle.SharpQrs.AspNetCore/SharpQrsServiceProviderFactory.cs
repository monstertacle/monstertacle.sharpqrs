using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

/// <summary>Prepares the final host composition before delegating provider creation.</summary>
/// <remarks>The inner factory must accept a read-only IServiceCollection and must not add registrations during provider creation.
/// The wrapper keeps no per-build mutable state; concurrent use follows the inner factory's contract. The host/caller owns each returned provider.</remarks>
public sealed class SharpQrsServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly IServiceProviderFactory<IServiceCollection> inner;

    /// <summary>Wraps a host-supplied provider factory without creating providers or services.</summary>
    /// <param name="inner">The non-null factory owning actual provider construction.</param>
    /// <exception cref="ArgumentNullException">The inner factory is null.</exception>
    public SharpQrsServiceProviderFactory(IServiceProviderFactory<IServiceCollection> inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        this.inner = inner;
    }

    /// <summary>Delegates builder creation. Host container configuration may still change the builder.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The inner factory's builder.</returns>
    /// <exception cref="ArgumentNullException">The service collection is null.</exception>
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return inner.CreateBuilder(services);
    }

    /// <summary>Validates the final builder and delegates construction using an independent read-only copy.</summary>
    /// <param name="containerBuilder">The final configured builder.</param>
    /// <returns>The provider owned by the caller/host.</returns>
    /// <exception cref="ArgumentNullException">The builder is null.</exception>
    /// <exception cref="InvalidOperationException">The final composition fails validation.</exception>
    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder) => inner.CreateServiceProvider(containerBuilder.PrepareSharpQrs());
}
