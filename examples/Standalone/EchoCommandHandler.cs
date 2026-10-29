namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed class EchoCommandHandler : ICommandHandler<EchoCommand, EchoResult>, ISyncCommandHandler<EchoCommand, EchoResult>
{
    internal int Calls { get; private set; }
    public Task<EchoResult> HandleAsync(EchoCommand request, CancellationToken cancellationToken = default)
        => Task.FromResult(Handle(request, cancellationToken));

    public EchoResult Handle(EchoCommand request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return new EchoResult(request.Value);
    }
}
