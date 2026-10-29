namespace Monstertacle.SharpQrs;

/// <summary>Configures synchronous query mappings before dispatch begins.</summary>
public interface ISyncQueryRegistration
{
    /// <summary>Registers one exact mapping without taking ownership.</summary>
    /// <typeparam name="TQuery">The exact request type.</typeparam>
    /// <typeparam name="TResult">The non-null result contract.</typeparam>
    /// <param name="handler">The caller-owned handler.</param>
    void Register<TQuery, TResult>(ISyncQueryHandler<TQuery, TResult> handler)
        where TQuery : IQuery<TResult>;
}
