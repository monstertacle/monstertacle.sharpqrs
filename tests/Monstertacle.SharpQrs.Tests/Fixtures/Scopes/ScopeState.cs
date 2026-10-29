using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

public sealed class ScopeState : IDisposable
{
    public Guid Id { get; } = Guid.NewGuid();
    public TaskCompletionSource Disposed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Disposals { get; private set; }
    public void Dispose()
    {
        Disposals++;
        Disposed.TrySetResult();
    }
}
