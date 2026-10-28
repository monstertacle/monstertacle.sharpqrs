namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class AlternativeEchoHandler : ICommandHandler<EchoCommand, EchoResult>, IQueryHandler<EchoQuery, EchoResult>
{
    public Task<EchoResult> HandleAsync(EchoCommand command, CancellationToken cancellationToken = default) => Task.FromResult(new EchoResult("alternative"));
    public Task<EchoResult> HandleAsync(EchoQuery query, CancellationToken cancellationToken = default) => Task.FromResult(new EchoResult("alternative"));
}
