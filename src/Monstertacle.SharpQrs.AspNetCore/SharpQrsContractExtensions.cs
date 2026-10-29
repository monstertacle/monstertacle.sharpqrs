using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

/// <summary>Selects existing unkeyed handler services through their contracts.</summary>
public static class SharpQrsContractExtensions
{
    /// <summary>Selects one existing command handler service without activating it.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="services">The setup-only service collection.</param>
    /// <returns>The original collection.</returns>
    public static IServiceCollection AddSharpQrsCommand<TCommand, TResult>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
    {
        RegistrationValidation.Infrastructure(services);
        HandlerServiceSelection.Validate<ICommandHandler<TCommand, TResult>>(services);
        RegistrationValidation.Mapping(services, typeof(TCommand), false);
        services.AddSharpQrs();
        services.AddSingleton(new CommandHandlerRegistration(typeof(TCommand), (dispatcher, provider) =>
        {
            HandlerServiceSelection.Validate<ICommandHandler<TCommand, TResult>>(services);
            dispatcher.Register(new ScopedCommandHandler<TCommand, TResult, ICommandHandler<TCommand, TResult>>(provider));
        }));
        return services;
    }

    /// <summary>Selects one existing query handler service without activating it.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="services">The setup-only service collection.</param>
    /// <returns>The original collection.</returns>
    public static IServiceCollection AddSharpQrsQuery<TQuery, TResult>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
    {
        RegistrationValidation.Infrastructure(services);
        HandlerServiceSelection.Validate<IQueryHandler<TQuery, TResult>>(services);
        RegistrationValidation.Mapping(services, typeof(TQuery), true);
        services.AddSharpQrs();
        services.AddSingleton(new QueryHandlerRegistration(typeof(TQuery), (dispatcher, provider) =>
        {
            HandlerServiceSelection.Validate<IQueryHandler<TQuery, TResult>>(services);
            dispatcher.Register(new ScopedQueryHandler<TQuery, TResult, IQueryHandler<TQuery, TResult>>(provider));
        }));
        return services;
    }
}
