using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Http;

public sealed class HttpScopeHost(WebApplication application, HttpClient client, ConcurrentQueue<ScopedHandler> handlers) : IAsyncDisposable
{
    public HttpClient Client => client;
    public ConcurrentQueue<ScopedHandler> Handlers => handlers;

    public static async Task<HttpScopeHost> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Host.UseDefaultServiceProvider(options => options.ValidateScopes = true);
        builder.Services.AddScopedProbe();
        var app = builder.Build();
        var observed = new ConcurrentQueue<ScopedHandler>();
        app.Run(context => HandleAsync(context, observed));
        await app.StartAsync();
        return new HttpScopeHost(app, app.GetTestClient(), observed);
    }

    private static async Task HandleAsync(HttpContext context, ConcurrentQueue<ScopedHandler> observed)
    {
        var command = await context.RequestServices.DispatchScope(false);
        var query = await context.RequestServices.DispatchScope(true);
        observed.Enqueue(context.RequestServices.GetRequiredService<ScopedHandler>());
        await context.Response.WriteAsync($"{command};{query}", context.RequestAborted);
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();
        await application.DisposeAsync();
    }
}
