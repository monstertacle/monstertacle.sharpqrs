namespace Monstertacle.SharpQrs.Tests.Fixtures;

public record EchoQuery(string Value) : IQuery<EchoResult>;
