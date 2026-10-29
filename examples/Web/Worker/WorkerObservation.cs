namespace Monstertacle.SharpQrs.Examples.Web.Worker;

public sealed record WorkerObservation(Guid CommandScope, Guid QueryScope, int Disposals, int Decorations);
