using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;
using Monstertacle.SharpQrs.Tests.Fixtures;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class CompositionCopyTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CustomisedCopiesValidateTheirOwnFinalSelections(int mode)
    {
        var original = new ServiceCollection();
        var activations = 0;
        ContractSetup.AddHandler(original, mode, 1, () => activations++);
        ContractSetup.Select(original, mode);
        var handlerType = original.First().ServiceType;
        foreach (var count in new[] { 0, 1, 2 })
        {
            IServiceCollection copy = new ServiceCollection();
            foreach (var descriptor in original.Where(d => d.ServiceType != handlerType)) copy.Add(descriptor);
            for (var index = 0; index < count; index++) ContractSetup.AddHandler(copy, mode, 2, () => activations++);
            var before = copy.ToArray();
            if (count != 1)
            {
                Assert.Throws<InvalidOperationException>(() => copy.PrepareSharpQrs());
                Assert.Equal(before, copy);
                continue;
            }
            var prepared = copy.PrepareSharpQrs();
            Assert.NotSame(copy, prepared);
            Assert.True(prepared.IsReadOnly);
            Assert.Equal(0, activations);
            using var provider = prepared.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            using var scope = provider.CreateScope();
            Assert.Equal(2, await ContractDispatch.Execute(scope.ServiceProvider, mode));
        }
        Assert.Equal(1, activations);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task OriginalChangesCannotInvalidateAnIndependentCopy(int mode)
    {
        var original = new ServiceCollection();
        ContractSetup.AddHandler(original, mode);
        ContractSetup.Select(original, mode);
        IServiceCollection copy = new ServiceCollection();
        foreach (var descriptor in original) copy.Add(descriptor);
        original.Clear();
        using var first = copy.PrepareSharpQrs().BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var second = copy.PrepareSharpQrs().BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var firstScope = first.CreateScope();
        using var secondScope = second.CreateScope();
        Assert.Equal(42, await ContractDispatch.Execute(firstScope.ServiceProvider, mode));
        Assert.Equal(42, await ContractDispatch.Execute(secondScope.ServiceProvider, mode));
    }
}
