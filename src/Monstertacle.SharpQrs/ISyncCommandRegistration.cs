namespace Monstertacle.SharpQrs;

/// <summary>Configures synchronous command mappings before dispatch begins.</summary>
public interface ISyncCommandRegistration
{
    /// <summary>Registers one exact mapping without taking ownership.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The non-null result contract.</typeparam>
    /// <param name="handler">The caller-owned handler.</param>
    void Register<TCommand, TResult>(ISyncCommandHandler<TCommand, TResult> handler)
        where TCommand : ICommand<TResult>;
}
