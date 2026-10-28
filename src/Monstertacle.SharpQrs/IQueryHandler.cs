namespace Monstertacle.SharpQrs;

/// <summary>Handles a query. Applications control thread safety and resource ownership.</summary>
/// <typeparam name="TQuery">The supported request contract.</typeparam>
/// <typeparam name="TResult">The result type. Every actual null result is rejected.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>Executes once with the original token, including an already-cancelled token.</summary>
    /// <param name="query">The non-null request.</param>
    /// <param name="cancellationToken">The caller's token. The handler controls cooperative cancellation.</param>
    /// <returns>A non-null task completing with a non-null result.</returns>
    /// <remarks>Exceptions and cancellation propagate unchanged. Implementations may receive overlapping calls.</remarks>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
