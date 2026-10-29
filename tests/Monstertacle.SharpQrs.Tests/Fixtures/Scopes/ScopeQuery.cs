using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Tests.Fixtures.Scopes;

public sealed record ScopeQuery : IQuery<Guid>;
