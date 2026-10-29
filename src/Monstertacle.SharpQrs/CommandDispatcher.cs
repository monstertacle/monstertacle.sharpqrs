namespace Monstertacle.SharpQrs;

/// <summary>A standalone command dispatcher with instance-local registrations and caller-owned handlers.</summary>
/// <remarks>Finish registration before dispatch. Concurrent registration is unsupported; concurrent dispatch does not imply handler thread safety.</remarks>
public sealed class CommandDispatcher : ICommandDispatcher, ICommandRegistration
{
    private readonly DispatchRegistry registry = new();

    /// <summary>Creates an empty dispatcher.</summary>
    public CommandDispatcher() { }

    /// <summary>Registers one exact request/result mapping, retaining the caller-owned handler.</summary>
    /// <typeparam name="TCommand">The exact concrete request type.</typeparam>
    /// <typeparam name="TResult">The result type; actual null results are prohibited.</typeparam>
    /// <param name="handler">The non-null handler; the caller remains responsible for its lifetime.</param>
    /// <exception cref="ArgumentNullException">The handler is null.</exception>
    /// <exception cref="InvalidOperationException">This request already has a registration. The original is preserved.</exception>
    public void Register<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler)
        where TCommand : ICommand<TResult>
    {
        ArgumentNullException.ThrowIfNull(handler);
        registry.Register<TCommand, TResult>(handler.HandleAsync);
    }

    /// <inheritdoc />
    public Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => registry.DispatchAsync<TResult>(command, cancellationToken);
}
