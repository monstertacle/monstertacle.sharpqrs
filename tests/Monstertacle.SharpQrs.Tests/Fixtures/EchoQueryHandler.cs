namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class EchoQueryHandler : IQueryHandler<EchoQuery, EchoResult>
{
    public int Calls { get; private set; }

    public Task<EchoResult> HandleAsync(EchoQuery request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(new EchoResult(request.Value));
    }
}
