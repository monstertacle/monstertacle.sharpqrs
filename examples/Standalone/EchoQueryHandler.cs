namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed class EchoQueryHandler : IQueryHandler<EchoQuery, EchoResult>, ISyncQueryHandler<EchoQuery, EchoResult>
{
    internal int Calls { get; private set; }
    public Task<EchoResult> HandleAsync(EchoQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(Handle(request, cancellationToken));

    public EchoResult Handle(EchoQuery request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return new EchoResult(request.Value);
    }
}
