using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Examples.Web.Worker;

public static class WorkerDemo
{
    public static Task<IReadOnlyList<WorkerObservation>> RunAsync(CancellationToken cancellationToken = default)
        => RunAsync(CreateBuilder(), cancellationToken);

    public static IHostBuilder CreateBuilder()
        => Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(ExampleComposition.CreateFactory())
            .ConfigureServices(ConfigureServices);

    public static async Task<IReadOnlyList<WorkerObservation>> RunAsync(IHostBuilder builder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        cancellationToken.ThrowIfCancellationRequested();
        using var host = builder.Build();
        await host.StartAsync(cancellationToken);
        try { return await host.Services.GetRequiredService<WorkerResults>().WaitAsync(cancellationToken); }
        finally { await host.StopAsync(CancellationToken.None); }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<WorkerResults>();
        services.AddScoped<WorkerUnit>();
        services.AddScoped<WorkerOperation>();
        services.AddScoped<WorkerCommandHandler>();
        services.AddSharpQrsCommand<WorkerRequest, Guid, WorkerCommandDecorator>();
        services.AddSharpQrsQuery<WorkerRequest, Guid, WorkerQueryHandler>();
        services.AddHostedService<ScopedDispatchWorker>();
    }
}
