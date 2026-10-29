namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record SyncCommandHandlerRegistration(Type RequestType, Action<SyncCommandDispatcher, IServiceProvider> Install);
