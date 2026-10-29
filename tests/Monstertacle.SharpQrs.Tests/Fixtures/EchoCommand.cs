namespace Monstertacle.SharpQrs.Tests.Fixtures;

public record EchoCommand(string Value) : ICommand<EchoResult>;
