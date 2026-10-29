using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

public sealed class ScopedHandler(ScopeState state) : ICommandHandler<ScopeCommand, Guid>, IQueryHandler<ScopeQuery, Guid>, IDisposable
{
    public ScopeState State => state;
    public int Disposals { get; private set; }
    public Task<Guid> HandleAsync(ScopeCommand command, CancellationToken cancellationToken = default) => Task.FromResult(state.Id);
    public Task<Guid> HandleAsync(ScopeQuery query, CancellationToken cancellationToken = default) => Task.FromResult(state.Id);
    public void Dispose() => Disposals++;
}
