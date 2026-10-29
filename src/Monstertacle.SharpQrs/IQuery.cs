namespace Monstertacle.SharpQrs;

/// <summary>A typed query whose result must be non-null at runtime.</summary>
/// <typeparam name="TResult">The invariant result contract; absence requires a non-null representation.</typeparam>
public interface IQuery<TResult> { }
