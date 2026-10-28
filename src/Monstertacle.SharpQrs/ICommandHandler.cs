namespace Monstertacle.SharpQrs;

/// <summary>Handles a command. Applications control thread safety and resource ownership.</summary>
/// <typeparam name="TCommand">The supported request contract.</typeparam>
/// <typeparam name="TResult">The result type. Every actual null result is rejected.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>Executes once with the original token, including an already-cancelled token.</summary>
    /// <param name="command">The non-null request.</param>
    /// <param name="cancellationToken">The caller's token. The handler controls cooperative cancellation.</param>
    /// <returns>A non-null task completing with a non-null result.</returns>
    /// <remarks>Exceptions and cancellation propagate unchanged. Implementations may receive overlapping calls.</remarks>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
