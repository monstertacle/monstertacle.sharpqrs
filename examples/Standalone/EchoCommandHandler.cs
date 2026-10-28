namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed class EchoCommandHandler : ICommandHandler<EchoCommand, EchoResult>
{
    internal int Calls { get; private set; }
    public Task<EchoResult> HandleAsync(EchoCommand request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(new EchoResult(request.Value));
    }
}
