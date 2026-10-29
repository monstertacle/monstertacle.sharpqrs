using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

/// <summary>Configures explicit, scoped synchronous CQRS without handler activation.</summary>
public static class SharpQrsSynchronousExtensions
{
    /// <summary>Registers synchronous dispatch infrastructure once.</summary>
    /// <param name="services">The setup-only collection.</param>
    /// <returns>The original collection.</returns>
    public static IServiceCollection AddSharpQrsSynchronous(this IServiceCollection services)
    {
        SyncRegistrationValidation.Infrastructure(services);
        if (!services.Any(d => d.ServiceType == typeof(SyncInfrastructureRegistration)))
        {
            var commands = ServiceDescriptor.Scoped<ISyncCommandDispatcher>(SyncScopedDispatcherFactory.CreateCommand);
            var queries = ServiceDescriptor.Scoped<ISyncQueryDispatcher>(SyncScopedDispatcherFactory.CreateQuery);
            services.Add(commands);
            services.Add(queries);
            services.AddSingleton(new SyncInfrastructureRegistration(commands, queries));
        }
        return services;
    }

    /// <summary>Selects one exact synchronous command mapping with lazy scope resolution.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <param name="services">The collection to configure before provider construction.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping or service selection conflicts; descriptors are preserved.</exception>
    /// <remarks>Container-owned services activate only on dispatch. Use asynchronous scope disposal for async-only resources.
    /// Registration does not guarantee handler thread safety. Contract selection requires exactly one unkeyed service.</remarks>
    public static IServiceCollection AddSharpQrsSyncCommand<TCommand, TResult>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
    {
        SyncRegistrationValidation.Infrastructure(services);
        HandlerServiceSelection.Validate<ISyncCommandHandler<TCommand, TResult>>(services);
        SyncRegistrationValidation.Mapping(services, typeof(TCommand), false);
        services.AddSharpQrsSynchronous();
        services.AddSingleton(new SyncCommandHandlerRegistration(typeof(TCommand), (dispatcher, provider) =>
        {
            HandlerServiceSelection.Validate<ISyncCommandHandler<TCommand, TResult>>(services);
            dispatcher.Register(new ScopedSyncCommandHandler<TCommand, TResult, ISyncCommandHandler<TCommand, TResult>>(provider));
        }));
        return services;
    }

    /// <summary>Selects one exact synchronous command mapping with lazy scope resolution.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <typeparam name="THandler">A concrete closed scoped handler.</typeparam>
    /// <param name="services">The collection to configure before provider construction.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping or service selection conflicts; descriptors are preserved.</exception>
    /// <remarks>Container-owned services activate only on dispatch. Use asynchronous scope disposal for async-only resources.
    /// Registration does not guarantee handler thread safety. Contract selection requires exactly one unkeyed service.</remarks>
    public static IServiceCollection AddSharpQrsSyncCommand<TCommand, TResult, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
        where THandler : class, ISyncCommandHandler<TCommand, TResult>
    {
        SyncRegistrationValidation.Concrete(services, typeof(THandler));
        SyncRegistrationValidation.Mapping(services, typeof(TCommand), false);
        services.AddSharpQrsSynchronous();
        if (!services.Any(d => d.ServiceType == typeof(THandler)))
        {
            services.AddScoped<THandler>();
            services.AddSingleton(new RegisteredHandlerType(typeof(THandler)));
        }
        services.AddSingleton(new SyncCommandHandlerRegistration(typeof(TCommand),
            (dispatcher, provider) => dispatcher.Register(new ScopedSyncCommandHandler<TCommand, TResult, THandler>(provider))));
        return services;
    }

    /// <summary>Selects one exact synchronous query mapping with lazy scope resolution.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <param name="services">The collection to configure before provider construction.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping or service selection conflicts; descriptors are preserved.</exception>
    /// <remarks>Container-owned services activate only on dispatch. Use asynchronous scope disposal for async-only resources.
    /// Registration does not guarantee handler thread safety. Contract selection requires exactly one unkeyed service.</remarks>
    public static IServiceCollection AddSharpQrsSyncQuery<TQuery, TResult>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
    {
        SyncRegistrationValidation.Infrastructure(services);
        HandlerServiceSelection.Validate<ISyncQueryHandler<TQuery, TResult>>(services);
        SyncRegistrationValidation.Mapping(services, typeof(TQuery), true);
        services.AddSharpQrsSynchronous();
        services.AddSingleton(new SyncQueryHandlerRegistration(typeof(TQuery), (dispatcher, provider) =>
        {
            HandlerServiceSelection.Validate<ISyncQueryHandler<TQuery, TResult>>(services);
            dispatcher.Register(new ScopedSyncQueryHandler<TQuery, TResult, ISyncQueryHandler<TQuery, TResult>>(provider));
        }));
        return services;
    }

    /// <summary>Selects one exact synchronous query mapping with lazy scope resolution.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <typeparam name="THandler">A concrete closed scoped handler.</typeparam>
    /// <param name="services">The collection to configure before provider construction.</param>
    /// <returns>The original collection.</returns>
    /// <exception cref="ArgumentNullException">The collection is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping or service selection conflicts; descriptors are preserved.</exception>
    /// <remarks>Container-owned services activate only on dispatch. Use asynchronous scope disposal for async-only resources.
    /// Registration does not guarantee handler thread safety. Contract selection requires exactly one unkeyed service.</remarks>
    public static IServiceCollection AddSharpQrsSyncQuery<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
        where THandler : class, ISyncQueryHandler<TQuery, TResult>
    {
        SyncRegistrationValidation.Concrete(services, typeof(THandler));
        SyncRegistrationValidation.Mapping(services, typeof(TQuery), true);
        services.AddSharpQrsSynchronous();
        if (!services.Any(d => d.ServiceType == typeof(THandler)))
        {
            services.AddScoped<THandler>();
            services.AddSingleton(new RegisteredHandlerType(typeof(THandler)));
        }
        services.AddSingleton(new SyncQueryHandlerRegistration(typeof(TQuery),
            (dispatcher, provider) => dispatcher.Register(new ScopedSyncQueryHandler<TQuery, TResult, THandler>(provider))));
        return services;
    }
}
