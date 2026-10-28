namespace Monstertacle.SharpQrs.Tests.Fixtures;

public abstract class AbstractEchoHandler : ICommandHandler<EchoCommand, EchoResult>, IQueryHandler<EchoQuery, EchoResult>
{
    public abstract Task<EchoResult> HandleAsync(EchoCommand command, CancellationToken cancellationToken = default);
    public abstract Task<EchoResult> HandleAsync(EchoQuery query, CancellationToken cancellationToken = default);
}
