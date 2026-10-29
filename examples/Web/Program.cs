namespace Monstertacle.SharpQrs.Examples.Web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Contains("--worker", StringComparer.Ordinal))
        {
            var completed = await Worker.WorkerDemo.RunAsync();
            Console.WriteLine($"Worker completed {completed.Count} scoped units with a decorated command and query.");
            return;
        }
        await using var app = CreateApplication(args);
        await app.RunAsync();
    }

    public static WebApplication CreateApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var urls = builder.Configuration["urls"] ?? "http://127.0.0.1:5080";
        if (urls.Split(';').Any(url => !new Uri(url).IsLoopback))
            throw new InvalidOperationException("The demonstration must bind to loopback addresses.");
        builder.WebHost.UseUrls(urls);
        builder.Host.UseServiceProviderFactory(ExampleComposition.CreateFactory());
        ExampleComposition.ConfigureWeb(builder.Services);
        var app = builder.Build();
        app.MapPost("/demo/commands", (EchoCommand command, ICommandDispatcher dispatcher, CancellationToken token)
            => dispatcher.DispatchAsync(command, token));
        app.MapGet("/demo/queries/{value}", (string value, IQueryDispatcher dispatcher, CancellationToken token)
            => dispatcher.DispatchAsync(new EchoQuery(value), token));
        return app;
    }
}
