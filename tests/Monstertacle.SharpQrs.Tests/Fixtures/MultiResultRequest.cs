namespace Monstertacle.SharpQrs.Tests.Fixtures;

public sealed record MultiResultRequest : ICommand<int>, ICommand<string>, IQuery<int>, IQuery<string>;
