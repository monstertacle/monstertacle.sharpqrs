namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class MultiResultHandler : ICommandHandler<MultiResultRequest, int>, IQueryHandler<MultiResultRequest, int>
{
    public int Calls { get; private set; }
    public Task<int> HandleAsync(MultiResultRequest request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return Task.FromResult(42);
    }
}
