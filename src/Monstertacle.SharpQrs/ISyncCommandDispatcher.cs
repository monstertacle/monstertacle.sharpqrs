namespace Monstertacle.SharpQrs;

/// <summary>Dispatches synchronous commands by exact concrete request and result type.</summary>
public interface ISyncCommandDispatcher
{
    /// <summary>Invokes the matching synchronous handler once; never adapts asynchronous mappings.</summary>
    /// <typeparam name="TResult">The exact result contract.</typeparam>
    /// <param name="command">The non-null request.</param>
    /// <param name="cancellationToken">Forwarded unchanged; the handler controls cancellation.</param>
    /// <returns>The non-null result.</returns>
    /// <exception cref="ArgumentNullException">The request is null.</exception>
    /// <exception cref="InvalidOperationException">The exact mapping is missing, its result contract differs or its result is null.</exception>
    /// <remarks>Finish registration before dispatch. Concurrent calls require application-owned handler thread safety.
    /// Original handler exceptions and cancellation propagate unchanged. The dispatcher never owns handlers.</remarks>
    TResult Dispatch<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}
