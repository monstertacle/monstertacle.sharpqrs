using System.Collections.Frozen;

namespace Monstertacle.SharpQrs.AspNetCore;

internal sealed class PreparedComposition(IEnumerable<Type> serviceTypes)
{
    private readonly FrozenSet<Type> services = serviceTypes.ToFrozenSet();
    internal bool Contains(Type serviceType) => services.Contains(serviceType);
}
