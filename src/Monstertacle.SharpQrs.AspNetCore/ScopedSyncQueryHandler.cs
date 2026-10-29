using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed class ScopedSyncQueryHandler<TRequest, TResult, THandler>(IServiceProvider provider)
    : ISyncQueryHandler<TRequest, TResult>
    where TRequest : IQuery<TResult>
    where THandler : class, ISyncQueryHandler<TRequest, TResult>
{
    public TResult Handle(TRequest request, CancellationToken cancellationToken = default)
        => provider.GetRequiredService<THandler>().Handle(request, cancellationToken);
}
