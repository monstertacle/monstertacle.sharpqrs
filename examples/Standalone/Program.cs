namespace Monstertacle.SharpQrs.Examples.Standalone;

internal static class Program
{
    private static async Task Main()
    {
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        var command = new EchoCommandHandler();
        var query = new EchoQueryHandler();
        commands.Register(command);
        queries.Register(query);
        var written = await commands.DispatchAsync(new EchoCommand("example"));
        var read = await queries.DispatchAsync(new EchoQuery("example"));
        if (written.Value != "example" || read.Value != "example" || command.Calls != 1 || query.Calls != 1)
            throw new InvalidOperationException("Standalone dispatch failed.");
        Console.WriteLine("Standalone command and query passed; handlers remain caller-owned.");
    }
}
