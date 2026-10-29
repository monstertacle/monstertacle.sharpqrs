using Microsoft.Extensions.DependencyInjection;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed record InfrastructureRegistration(ServiceDescriptor Commands, ServiceDescriptor Queries);
