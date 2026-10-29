namespace Monstertacle.SharpQrs.Tests.Fixtures;

public record ProbeRequest<TResult>(int Id = 0) : ICommand<TResult>, IQuery<TResult>;
