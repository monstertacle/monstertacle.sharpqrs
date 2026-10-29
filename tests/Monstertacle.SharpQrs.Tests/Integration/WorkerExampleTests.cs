using Monstertacle.SharpQrs.Examples.Web.Worker;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class WorkerExampleTests
{
    [Fact]
    public async Task DecoratedWorkerUsesOneAsyncScopePerUnitAndDisposesItsServices()
    {
        var units = await WorkerDemo.RunAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(2, units.Count);
        Assert.NotEqual(units[0].CommandScope, units[1].CommandScope);
        Assert.All(units, unit =>
        {
            Assert.NotEqual(Guid.Empty, unit.CommandScope);
            Assert.Equal(unit.CommandScope, unit.QueryScope);
            Assert.Equal(1, unit.Disposals);
            Assert.Equal(1, unit.Decorations);
        });
    }

    [Fact]
    public async Task WorkerHonoursTheCallersCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => WorkerDemo.RunAsync(cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
    }

    [Fact]
    public async Task WorkerFailureReachesTheCallerWithoutWrapping()
    {
        var error = new InvalidOperationException("worker dependency failure");
        var builder = WorkerDemo.CreateBuilder().ConfigureServices(services =>
            services.Replace(ServiceDescriptor.Scoped<WorkerUnit>(_ => throw error)));
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            WorkerDemo.RunAsync(builder).WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Same(error, actual);
    }

    [Fact]
    public async Task WorkerCommandLineFinishesWithoutStartingTheHttpHost()
    {
        var error = await Record.ExceptionAsync(() =>
            Examples.Web.Program.Main(["--worker"]).WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.Null(error);
    }
}
