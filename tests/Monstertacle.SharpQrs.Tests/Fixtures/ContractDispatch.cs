using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.Tests.Fixtures;

public static class ContractDispatch
{
    public static Task<int> Execute(IServiceProvider provider, int mode)
        => mode switch
        {
            0 => provider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new ProbeRequest<int>()),
            1 => provider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new ProbeRequest<int>()),
            2 => Task.FromResult(provider.GetRequiredService<ISyncCommandDispatcher>().Dispatch(new ProbeRequest<int>())),
            3 => Task.FromResult(provider.GetRequiredService<ISyncQueryDispatcher>().Dispatch(new ProbeRequest<int>())),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
}
