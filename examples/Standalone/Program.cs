namespace Monstertacle.SharpQrs.Examples.Standalone;

public static class Program
{
    public static async Task Main()
    {
        var commands = new CommandDispatcher();
        var queries = new QueryDispatcher();
        var command = new EchoCommandHandler();
        var query = new EchoQueryHandler();
        ((ICommandRegistration)commands).Register(command);
        ((IQueryRegistration)queries).Register(query);
        var synchronousCommands = new SyncCommandDispatcher();
        var synchronousQueries = new SyncQueryDispatcher();
        ((ISyncCommandRegistration)synchronousCommands).Register(command);
        ((ISyncQueryRegistration)synchronousQueries).Register(query);
        var syncWritten = synchronousCommands.Dispatch(new EchoCommand("sync"));
        var syncRead = synchronousQueries.Dispatch(new EchoQuery("sync"));
        if (syncWritten.Value != "sync" || syncRead.Value != "sync")
            throw new InvalidOperationException("Synchronous standalone dispatch failed.");
        var written = await commands.DispatchAsync(new EchoCommand("example"));
        var read = await queries.DispatchAsync(new EchoQuery("example"));
        if (written.Value != "example" || read.Value != "example" || command.Calls != 2 || query.Calls != 2)
            throw new InvalidOperationException("Standalone dispatch failed.");
        Console.WriteLine("Standalone synchronous and asynchronous commands and queries passed; handlers remain caller-owned.");
    }
}
