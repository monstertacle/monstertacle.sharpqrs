using Monstertacle.SharpQrs;

namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed record EchoCommand(string Value) : ICommand<EchoResult>;
