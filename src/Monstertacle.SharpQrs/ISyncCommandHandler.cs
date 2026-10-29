namespace Monstertacle.SharpQrs;

/// <summary>Handles synchronous commands without task adaptation. Applications own thread safety and lifetimes.</summary>
/// <typeparam name="TCommand">The request contract.</typeparam>
/// <typeparam name="TResult">The result contract; actual null results are rejected.</typeparam>
public interface ISyncCommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>Executes once; original errors and cooperative cancellation propagate unchanged.</summary>
    /// <param name="command">The non-null request.</param>
    /// <param name="cancellationToken">Forwarded unchanged, including already-cancelled tokens.</param>
    /// <returns>A non-null result.</returns>
    TResult Handle(TCommand command, CancellationToken cancellationToken = default);
}
