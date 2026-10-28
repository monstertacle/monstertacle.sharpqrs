using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed class ScopedQueryHandler<TRequest, TResult, THandler>(IServiceProvider provider)
    : IQueryHandler<TRequest, TResult>
    where TRequest : IQuery<TResult>
    where THandler : class, IQueryHandler<TRequest, TResult>
{
    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        => provider.GetRequiredService<THandler>().HandleAsync(request, cancellationToken);
}
