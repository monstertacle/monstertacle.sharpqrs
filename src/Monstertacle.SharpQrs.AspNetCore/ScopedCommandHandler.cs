using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed class ScopedCommandHandler<TRequest, TResult, THandler>(IServiceProvider provider)
    : ICommandHandler<TRequest, TResult>
    where TRequest : ICommand<TResult>
    where THandler : class, ICommandHandler<TRequest, TResult>
{
    public Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        => provider.GetRequiredService<THandler>().HandleAsync(request, cancellationToken);
}
