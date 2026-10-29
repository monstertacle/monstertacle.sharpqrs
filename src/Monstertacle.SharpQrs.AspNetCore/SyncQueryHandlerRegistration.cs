namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record SyncQueryHandlerRegistration(Type RequestType, Action<SyncQueryDispatcher, IServiceProvider> Install);
