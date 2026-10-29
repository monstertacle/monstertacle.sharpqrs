namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed class SyncMultiResultHandler : ISyncCommandHandler<MultiResultRequest, string>,
    ISyncQueryHandler<MultiResultRequest, string>, IDisposable
{
    public int Calls { get; private set; }
    public string Handle(MultiResultRequest request, CancellationToken cancellationToken = default)
    {
        Calls++;
        return "original";
    }
    public void Dispose() { }
}
