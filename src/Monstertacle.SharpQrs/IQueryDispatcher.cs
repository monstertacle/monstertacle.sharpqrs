namespace Monstertacle.SharpQrs;

/// <summary>Dispatches exact concrete query types after registration is complete.</summary>
public interface IQueryDispatcher
{
    /// <summary>Invokes the unique handler once; never takes ownership of the handler.</summary>
    /// <typeparam name="TResult">The exact registered result type.</typeparam>
    /// <param name="query">The non-null request.</param>
    /// <param name="cancellationToken">Forwarded unchanged; not intercepted before invocation.</param>
    /// <returns>The non-null handler result, without inspecting its members.</returns>
    /// <exception cref="ArgumentNullException">The request is null.</exception>
    /// <exception cref="InvalidOperationException">No exact registration exists, the result type differs, or the handler returns a null task or result.</exception>
    /// <remarks>Handler exceptions and cancellation propagate unchanged. Overlapping calls are supported, but handler thread safety is the application's responsibility.</remarks>
    Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
