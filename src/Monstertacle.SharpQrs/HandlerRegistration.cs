namespace Monstertacle.SharpQrs;

internal sealed record HandlerRegistration(Type ResultType, Delegate Invoke);
