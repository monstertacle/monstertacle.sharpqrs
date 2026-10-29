using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

public sealed class AsyncScopedHandler : ICommandHandler<ScopeCommand, Guid>, IQueryHandler<ScopeQuery, Guid>, IAsyncDisposable
{
    private readonly Guid id = Guid.NewGuid();
    public int Disposals { get; private set; }
    public Task<Guid> HandleAsync(ScopeCommand command, CancellationToken cancellationToken = default) => Task.FromResult(id);
    public Task<Guid> HandleAsync(ScopeQuery query, CancellationToken cancellationToken = default) => Task.FromResult(id);
    public ValueTask DisposeAsync()
    {
        Disposals++;
        return ValueTask.CompletedTask;
    }
}
