namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record CommandHandlerRegistration(Type RequestType, Action<CommandDispatcher, IServiceProvider> Install);
