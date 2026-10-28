namespace Monstertacle.SharpQrs.Examples.Web;

internal sealed class EchoQueryHandler(RequestScope scope) : IQueryHandler<EchoQuery, EchoResult>
{
    public Task<EchoResult> HandleAsync(EchoQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(new EchoResult(request.Value, scope.Id));
}
