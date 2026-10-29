using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures;

public static class ContractSetup
{
    public static void AddHandler(IServiceCollection services, int mode, int value = 42, Action? activated = null)
    {
        ProbeHandler<int> Async(IServiceProvider _) { activated?.Invoke(); return new((_, _) => Task.FromResult(value)); }
        SyncProbeHandler<int> Sync(IServiceProvider _) { activated?.Invoke(); return new((_, _) => value); }
        switch (mode)
        {
            case 0: services.AddScoped<ICommandHandler<ProbeRequest<int>, int>>(Async); break;
            case 1: services.AddScoped<IQueryHandler<ProbeRequest<int>, int>>(Async); break;
            case 2: services.AddScoped<ISyncCommandHandler<ProbeRequest<int>, int>>(Sync); break;
            case 3: services.AddScoped<ISyncQueryHandler<ProbeRequest<int>, int>>(Sync); break;
            default: throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    public static void Select(IServiceCollection services, int mode)
    {
        switch (mode)
        {
            case 0: services.AddSharpQrsCommand<ProbeRequest<int>, int>(); break;
            case 1: services.AddSharpQrsQuery<ProbeRequest<int>, int>(); break;
            case 2: services.AddSharpQrsSyncCommand<ProbeRequest<int>, int>(); break;
            case 3: services.AddSharpQrsSyncQuery<ProbeRequest<int>, int>(); break;
            default: throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    public static void SelectConcrete(IServiceCollection services, int mode)
    {
        switch (mode)
        {
            case 0: services.AddSharpQrsCommand<ProbeRequest<int>, int, CompositionHandler>(); break;
            case 1: services.AddSharpQrsQuery<ProbeRequest<int>, int, CompositionHandler>(); break;
            case 2: services.AddSharpQrsSyncCommand<ProbeRequest<int>, int, CompositionHandler>(); break;
            case 3: services.AddSharpQrsSyncQuery<ProbeRequest<int>, int, CompositionHandler>(); break;
            default: throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
