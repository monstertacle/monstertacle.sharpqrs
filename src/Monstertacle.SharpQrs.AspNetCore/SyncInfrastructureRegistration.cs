using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record SyncInfrastructureRegistration(ServiceDescriptor Commands, ServiceDescriptor Queries);
