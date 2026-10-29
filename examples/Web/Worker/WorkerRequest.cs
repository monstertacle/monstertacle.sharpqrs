namespace Monstertacle.SharpQrs.Examples.Web.Worker;

internal sealed record WorkerRequest : ICommand<Guid>, IQuery<Guid>;
