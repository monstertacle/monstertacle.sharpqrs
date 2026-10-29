using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Examples.Web;

public static class Program
{
    private static async Task Main(string[] args)
    {
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
        builder.Host.UseDefaultServiceProvider(options => options.ValidateScopes = true);
        builder.Services.AddScoped<RequestScope>();
        builder.Services.AddScoped<ICommandHandler<EchoCommand, EchoResult>, EchoCommandHandler>();
        builder.Services.AddScoped<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>();
        builder.Services.AddSharpQrsCommand<EchoCommand, EchoResult>();
        builder.Services.AddSharpQrsQuery<EchoQuery, EchoResult>();
        var app = builder.Build();
        app.MapPost("/demo/commands", (EchoCommand command, ICommandDispatcher dispatcher, CancellationToken token)
            => dispatcher.DispatchAsync(command, token));
        app.MapGet("/demo/queries/{value}", (string value, IQueryDispatcher dispatcher, CancellationToken token)
            => dispatcher.DispatchAsync(new EchoQuery(value), token));
        return app;
    }
}
