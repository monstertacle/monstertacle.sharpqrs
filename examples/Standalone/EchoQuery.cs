using Monstertacle.SharpQrs;

namespace Monstertacle.SharpQrs.Examples.Standalone;

internal sealed record EchoQuery(string Value) : IQuery<EchoResult>;
