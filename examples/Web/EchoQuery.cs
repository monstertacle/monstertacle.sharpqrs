namespace Monstertacle.SharpQrs.Examples.Web;

internal sealed record EchoQuery(string Value) : IQuery<EchoResult>;
