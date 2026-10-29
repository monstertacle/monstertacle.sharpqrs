namespace Monstertacle.SharpQrs;

/// <summary>Configures asynchronous command mappings before dispatch begins.</summary>
public interface ICommandRegistration
{
    /// <summary>Registers one exact mapping; the caller owns the handler.</summary>
    /// <typeparam name="TCommand">The exact request type.</typeparam>
    /// <typeparam name="TResult">The non-null result contract.</typeparam>
    /// <param name="handler">The caller-owned handler.</param>
    void Register<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler)
        where TCommand : ICommand<TResult>;
}
