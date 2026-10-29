using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class SynchronousRegistrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ConcreteDuplicateAndExternalRegistrationAreAtomic(bool query)
    {
        var services = new ServiceCollection();
        Register(services, query);
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Register(services, query));
        Assert.Equal(before, services);
        services = new ServiceCollection();
        services.AddScoped<SyncScopeHandler>();
        before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Register(services, query));
        Assert.Equal(before, services);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingAndAmbiguousAndLaterAmbiguousContractsFail(bool query)
    {
        var services = new ServiceCollection();
        Assert.Throws<InvalidOperationException>(() => Select(services, query));
        Assert.Empty(services);
        Add(services, query);
        Select(services, query);
        Add(services, query);
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => Select(services, query));
        Assert.Equal(before, services);
        Assert.Throws<InvalidOperationException>(() => services.BuildValidated());
    }

    [Fact]
    public void NullAndExternalInfrastructureAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => SharpQrsSynchronousExtensions.AddSharpQrsSynchronous(null!));
        Assert.Throws<ArgumentNullException>(() => SharpQrsSynchronousExtensions.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid>(null!));
        Assert.Throws<ArgumentNullException>(() => SharpQrsSynchronousExtensions.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid>(null!));
        var services = new ServiceCollection();
        services.AddScoped<ISyncCommandDispatcher, SyncCommandDispatcher>();
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.AddSharpQrsSynchronous());
        Assert.Equal(before, services);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExternalDescriptorsAddedAfterInfrastructureRemainConflicts(bool query)
    {
        var services = new ServiceCollection().AddSharpQrsSynchronous();
        if (query) services.AddScoped<ISyncQueryDispatcher, SyncQueryDispatcher>();
        else services.AddScoped<ISyncCommandDispatcher, SyncCommandDispatcher>();
        var before = services.ToArray();
        Assert.Throws<InvalidOperationException>(() => services.AddSharpQrsSynchronous());
        Assert.Equal(before, services);
    }

    private static void Register(IServiceCollection services, bool query)
    {
        if (query) services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
        else services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid, SyncScopeHandler>();
    }
    private static void Select(IServiceCollection services, bool query)
    {
        if (query) services.AddSharpQrsSyncQuery<ProbeRequest<Guid>, Guid>();
        else services.AddSharpQrsSyncCommand<ProbeRequest<Guid>, Guid>();
    }
    private static void Add(IServiceCollection services, bool query)
    {
        if (query) services.AddScoped<ISyncQueryHandler<ProbeRequest<Guid>, Guid>, SyncScopeHandler>();
        else services.AddScoped<ISyncCommandHandler<ProbeRequest<Guid>, Guid>, SyncScopeHandler>();
    }
}
