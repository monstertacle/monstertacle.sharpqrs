using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

/// <summary>Explicit, setup-only scoped CQRS registration. The container owns handlers and dependencies.</summary>
/// <remarks>No scanning, provider construction or handler activation occurs during registration.
/// Resolve dispatchers inside scopes and enable scope validation in the host. Thread safety of shared handlers remains the application's responsibility.</remarks>
public static class SharpQrsServiceCollectionExtensions
{
    /// <summary>Registers scoped dispatchers once, returning the original collection.</summary>
    /// <param name="services">The collection to configure before provider construction.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">An external dispatcher registration conflicts.</exception>
    public static IServiceCollection AddSharpQrs(this IServiceCollection services)
    {
        RegistrationValidation.Infrastructure(services);
        if (!services.Any(d => d.ServiceType == typeof(InfrastructureRegistration)))
        {
            var commands = ServiceDescriptor.Scoped<ICommandDispatcher>(ScopedDispatcherFactory.CreateCommand);
            var queries = ServiceDescriptor.Scoped<IQueryDispatcher>(ScopedDispatcherFactory.CreateQuery);
            services.Add(commands);
            services.Add(queries);
            services.AddSingleton(new InfrastructureRegistration(commands, queries));
        }
        return services;
    }

    /// <summary>Selects an exact command/result mapping and ensures scoped infrastructure.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The exact result type; every actual null result is rejected.</typeparam>
    /// <typeparam name="THandler">A concrete closed handler, registered only once even across multiple mappings.</typeparam>
    /// <param name="services">The collection to configure before building the provider.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping is duplicate, the handler is abstract/open, or external handler/dispatcher registrations conflict. Validation failure preserves descriptors.</exception>
    /// <remarks>Handlers activate lazily in the current scope. Scope disposal releases each concrete service once; use asynchronous disposal for async-only resources.
    /// Handler exceptions and cancellation propagate unchanged; registration does not promise handler thread safety.</remarks>
    public static IServiceCollection AddSharpQrsCommand<TCommand, TResult, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        RegistrationValidation.Handler(services, typeof(THandler));
        RegistrationValidation.Mapping(services, typeof(TCommand), false);
        services.AddSharpQrs();
        ConcreteHandlerRegistration.EnsureScoped<THandler>(services);
        services.AddSingleton(new CommandHandlerRegistration(typeof(TCommand),
            (dispatcher, provider) => dispatcher.Register(new ScopedCommandHandler<TCommand, TResult, THandler>(provider))));
        return services;
    }

    /// <summary>Selects an exact query/result mapping and ensures scoped infrastructure.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The exact result type; every actual null result is rejected.</typeparam>
    /// <typeparam name="THandler">A concrete closed handler, registered only once even across multiple mappings.</typeparam>
    /// <param name="services">The collection to configure before building the provider.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping is duplicate, the handler is abstract/open, or external handler/dispatcher registrations conflict. Validation failure preserves descriptors.</exception>
    /// <remarks>Handlers activate lazily in the current scope. Scope disposal releases each concrete service once; use asynchronous disposal for async-only resources.
    /// Handler exceptions and cancellation propagate unchanged; registration does not promise handler thread safety.</remarks>
    public static IServiceCollection AddSharpQrsQuery<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        RegistrationValidation.Handler(services, typeof(THandler));
        RegistrationValidation.Mapping(services, typeof(TQuery), true);
        services.AddSharpQrs();
        ConcreteHandlerRegistration.EnsureScoped<THandler>(services);
        services.AddSingleton(new QueryHandlerRegistration(typeof(TQuery),
            (dispatcher, provider) => dispatcher.Register(new ScopedQueryHandler<TQuery, TResult, THandler>(provider))));
        return services;
    }
}
