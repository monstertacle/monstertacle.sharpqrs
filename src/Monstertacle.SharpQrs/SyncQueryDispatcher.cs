namespace Monstertacle.SharpQrs;

/// <summary>A synchronous query dispatcher with instance-local, caller-owned mappings.</summary>
/// <remarks>Complete registration before dispatch. Concurrent dispatch does not imply handler thread safety.</remarks>
public sealed class SyncQueryDispatcher : ISyncQueryDispatcher, ISyncQueryRegistration
{
    private readonly SyncDispatchRegistry registry = new();

    /// <summary>Creates an empty synchronous dispatcher.</summary>
    public SyncQueryDispatcher() { }

    /// <summary>Registers one exact synchronous mapping, retaining caller ownership.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <param name="handler">The non-null, caller-owned handler.</param>
    /// <exception cref="ArgumentNullException">The handler is null.</exception>
    /// <exception cref="InvalidOperationException">The request already has a mapping; the original is preserved.</exception>
    public void Register<TQuery, TResult>(ISyncQueryHandler<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(handler);
        registry.Register<TQuery, TResult>(handler.Handle);
    }

    /// <inheritdoc />
    public TResult Dispatch<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => registry.Dispatch<TResult>(query, cancellationToken);
}
