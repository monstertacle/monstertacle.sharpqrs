using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class InvalidInputTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task NullRequestsAreRejectedExplicitly(bool query, bool integrated)
    {
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, _) => Task.FromResult(1));
        await Assert.ThrowsAsync<ArgumentNullException>(() => environment.DispatchAsync(null!));
        Assert.Equal(0, environment.Handler.Calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ABaseRegistrationDoesNotHandleDerivedRequests(bool query, bool integrated)
    {
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, _) => Task.FromResult(1));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => environment.DispatchAsync(new DerivedProbeRequest()));
        Assert.Contains(typeof(DerivedProbeRequest).FullName!, exception.Message);
        Assert.Equal(0, environment.Handler.Calls);
    }
}
