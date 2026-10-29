namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record QueryHandlerRegistration(Type RequestType, Action<QueryDispatcher, IServiceProvider> Install);
