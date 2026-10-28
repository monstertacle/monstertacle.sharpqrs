namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class EchoCommandHandler : ICommandHandler<EchoCommand, EchoResult>
{
    public int Calls { get; private set; }

    public Task<EchoResult> HandleAsync(EchoCommand request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(new EchoResult(request.Value));
    }
}
