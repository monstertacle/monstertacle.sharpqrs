using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Monstertacle.SharpQrs.Tests.Integration;

public sealed class ExampleTests
{
    [Fact]
    public async Task StandaloneExampleExercisesBothExecutionModes()
        => await Examples.Standalone.Program.Main();

    [Fact]
    public async Task WebExampleExecutesRealInterfaceMappingsInSeparateRequests()
    {
        await using var app = Examples.Web.Program.CreateApplication(["--urls", "http://127.0.0.1:0"]);
        await app.StartAsync();
        var server = app.Services.GetRequiredService<IServer>();
        var address = Assert.Single(server.Features.Get<IServerAddressesFeature>()!.Addresses);
        using var client = new HttpClient { BaseAddress = new Uri(address) };
        using var response = await client.PostAsJsonAsync("/demo/commands", new { Value = "command" });
        response.EnsureSuccessStatusCode();
        using var written = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        using var read = JsonDocument.Parse(await client.GetStringAsync("/demo/queries/query"));
        Assert.Equal("command", written.RootElement.GetProperty("value").GetString());
        Assert.Equal("query", read.RootElement.GetProperty("value").GetString());
        Assert.NotEqual(written.RootElement.GetProperty("scopeId").GetGuid(), read.RootElement.GetProperty("scopeId").GetGuid());
        await app.StopAsync();
    }

    [Fact]
    public void WebExampleRejectsNonLoopbackBindings()
        => Assert.Throws<InvalidOperationException>(() => Examples.Web.Program.CreateApplication(["--urls", "http://0.0.0.0:5080"]));
}
