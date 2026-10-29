using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record RegisteredHandlerType(Type HandlerType, ServiceDescriptor Descriptor);
