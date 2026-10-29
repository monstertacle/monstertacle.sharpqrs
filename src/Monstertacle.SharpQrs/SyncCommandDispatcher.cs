namespace Monstertacle.SharpQrs;

/// <summary>A synchronous command dispatcher with instance-local, caller-owned mappings.</summary>
/// <remarks>Complete registration before dispatch. Concurrent dispatch does not imply handler thread safety.</remarks>
public sealed class SyncCommandDispatcher : ISyncCommandDispatcher, ISyncCommandRegistration
{
    private readonly SyncDispatchRegistry registry = new();

    /// <summary>Creates an empty synchronous dispatcher.</summary>
    public SyncCommandDispatcher() { }

    /// <summary>Registers one exact synchronous mapping, retaining caller ownership.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The result contract.</typeparam>
    /// <param name="handler">The non-null, caller-owned handler.</param>
    /// <exception cref="ArgumentNullException">The handler is null.</exception>
    /// <exception cref="InvalidOperationException">The request already has a mapping; the original is preserved.</exception>
    public void Register<TCommand, TResult>(ISyncCommandHandler<TCommand, TResult> handler)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(handler);
        registry.Register<TCommand, TResult>(handler.Handle);
    }

    /// <inheritdoc />
    public TResult Dispatch<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => registry.Dispatch<TResult>(command, cancellationToken);
}
