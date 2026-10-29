using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class PreparationBoundaryTests
{
    [Theory]
    [InlineData(0, false)] [InlineData(1, false)] [InlineData(2, false)] [InlineData(3, false)]
    [InlineData(0, true)] [InlineData(1, true)] [InlineData(2, true)] [InlineData(3, true)]
    public async Task UnpreparedOrUnvalidatedNewMappingsFailBeforeActivation(int mode, bool staleEvidence)
    {
        IServiceCollection services = new ServiceCollection();
        if (staleEvidence)
            foreach (var descriptor in new ServiceCollection().PrepareSharpQrs()) services.Add(descriptor);
        var activations = 0;
        ContractSetup.AddHandler(services, mode, activated: () => activations++);
        ContractSetup.Select(services, mode);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ContractDispatch.Execute(scope.ServiceProvider, mode));
        Assert.Contains("PrepareSharpQrs", error.Message, StringComparison.Ordinal);
        Assert.Equal(0, activations);
    }

    [Fact]
    public async Task PublishedConcreteConstructionStillWorksWithoutPreparation()
    {
        var services = new ServiceCollection();
        for (var mode = 0; mode < 4; mode++) ContractSetup.SelectConcrete(services, mode);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        for (var mode = 0; mode < 4; mode++) Assert.Equal(42, await ContractDispatch.Execute(scope.ServiceProvider, mode));
    }

    [Fact]
    public void PreparedCopiesMustBePreparedAgainAfterCustomisation()
    {
        var original = new ServiceCollection();
        ContractSetup.AddHandler(original, 0);
        ContractSetup.Select(original, 0);
        var prepared = original.PrepareSharpQrs();
        IServiceCollection copy = new ServiceCollection();
        foreach (var descriptor in prepared) copy.Add(descriptor);
        ContractSetup.AddHandler(copy, 0);
        Assert.Throws<InvalidOperationException>(() => copy.PrepareSharpQrs());
        Assert.False(original.IsReadOnly);
        Assert.Throws<InvalidOperationException>(() => prepared.Clear());
    }
}
