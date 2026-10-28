using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Core;

public sealed class ResultContractTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task NullReferenceResultsAreRejected(bool query, bool integrated)
    {
        using var environment = new DispatchEnvironment<string?>(query, integrated, (_, _) => Task.FromResult<string?>(null));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => environment.DispatchAsync(new ProbeRequest<string?>()));
        Assert.Contains(typeof(ProbeRequest<string?>).FullName!, exception.Message);
        Assert.Contains("non-null", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task EmptyNullableValueResultsAreRejected(bool query, bool integrated)
    {
        using var environment = new DispatchEnvironment<int?>(query, integrated, (_, _) => Task.FromResult<int?>(null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => environment.DispatchAsync(new ProbeRequest<int?>()));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task NullableValuesAndNullableMembersAreAllowedWhenTopLevelResultIsNotNull(bool query, bool integrated)
    {
        using var values = new DispatchEnvironment<int?>(query, integrated, (_, _) => Task.FromResult<int?>(42));
        Assert.Equal(42, await values.DispatchAsync(new ProbeRequest<int?>()));
        using var members = new DispatchEnvironment<EchoResult>(query, integrated, (_, _) => Task.FromResult(new EchoResult(null)));
        Assert.Null((await members.DispatchAsync(new ProbeRequest<EchoResult>())).Value);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task NullTasksAreRejectedWithRequestIdentity(bool query, bool integrated)
    {
        using var environment = new DispatchEnvironment<int>(query, integrated, (_, _) => null!);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => environment.DispatchAsync(new ProbeRequest<int>()));
        Assert.Contains(typeof(ProbeRequest<int>).FullName!, exception.Message);
    }
}
