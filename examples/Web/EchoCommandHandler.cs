namespace Monstertacle.SharpQrs.Examples.Web;

internal sealed class EchoCommandHandler(RequestScope scope) : ICommandHandler<EchoCommand, EchoResult>
{
    public Task<EchoResult> HandleAsync(EchoCommand request, CancellationToken cancellationToken = default)
        => Task.FromResult(new EchoResult(request.Value, scope.Id));
}
