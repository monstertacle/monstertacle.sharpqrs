namespace Monstertacle.SharpQrs;

/// <summary>A standalone query dispatcher with instance-local registrations and caller-owned handlers.</summary>
/// <remarks>Finish registration before dispatch. Concurrent registration is unsupported; concurrent dispatch does not imply handler thread safety.</remarks>
public sealed class QueryDispatcher : IQueryDispatcher, IQueryRegistration
{
    private readonly DispatchRegistry registry = new();

    /// <summary>Creates an empty dispatcher.</summary>
    public QueryDispatcher() { }

    /// <summary>Registers one exact request/result mapping, retaining the caller-owned handler.</summary>
    /// <typeparam name="TQuery">The exact concrete request type.</typeparam>
    /// <typeparam name="TResult">The result type; actual null results are prohibited.</typeparam>
    /// <param name="handler">The non-null handler; the caller remains responsible for its lifetime.</param>
    /// <exception cref="ArgumentNullException">The handler is null.</exception>
    /// <exception cref="InvalidOperationException">This request already has a registration. The original is preserved.</exception>
    public void Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(handler);
        registry.Register<TQuery, TResult>(handler.HandleAsync);
    }

    /// <inheritdoc />
    public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => registry.DispatchAsync<TResult>(query, cancellationToken);
}
