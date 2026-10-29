namespace Monstertacle.SharpQrs;

/// <summary>Configures asynchronous query mappings before dispatch begins.</summary>
public interface IQueryRegistration
{
    /// <summary>Registers one exact mapping; the caller owns the handler.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The non-null result contract.</typeparam>
    /// <param name="handler">The caller-owned handler.</param>
    void Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>;
}
