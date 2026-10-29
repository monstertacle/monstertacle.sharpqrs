using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed class ScopedSyncCommandHandler<TRequest, TResult, THandler>(IServiceProvider provider)
    : ISyncCommandHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
    where THandler : class, ISyncCommandHandler<TRequest, TResult>
{
    public TResult Handle(TRequest request, CancellationToken cancellationToken = default)
        => provider.GetRequiredService<THandler>().Handle(request, cancellationToken);
}
