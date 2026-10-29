namespace Monstertacle.SharpQrs;

/// <summary>Handles synchronous querys without task adaptation. Applications own thread safety and lifetimes.</summary>
/// <typeparam name="TQuery">The request contract.</typeparam>
/// <typeparam name="TResult">The result contract; actual null results are rejected.</typeparam>
public interface ISyncQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>Executes once; original errors and cooperative cancellation propagate unchanged.</summary>
    /// <param name="query">The non-null request.</param>
    /// <param name="cancellationToken">Forwarded unchanged, including already-cancelled tokens.</param>
    /// <returns>A non-null result.</returns>
    TResult Handle(TQuery query, CancellationToken cancellationToken = default);
}
