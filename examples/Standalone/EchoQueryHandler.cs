namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed class EchoQueryHandler : IQueryHandler<EchoQuery, EchoResult>
{
    internal int Calls { get; private set; }
    public Task<EchoResult> HandleAsync(EchoQuery request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(new EchoResult(request.Value));
    }
}
