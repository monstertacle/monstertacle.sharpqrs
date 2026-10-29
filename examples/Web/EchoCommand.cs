namespace Monstertacle.SharpQrs.Examples.Web;

internal sealed record EchoCommand(string Value) : ICommand<EchoResult>;
