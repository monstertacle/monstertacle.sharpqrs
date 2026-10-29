namespace Monstertacle.SharpQrs;

internal sealed class DispatchRegistry
{
    private readonly Dictionary<Type, HandlerRegistration> registrations = [];

    internal void Register<TRequest, TResult>(Func<TRequest, CancellationToken, Task<TResult>> invoke)
    {
        Func<object, CancellationToken, Task<TResult>> bound = (request, token) => invoke((TRequest)request, token);
        if (!registrations.TryAdd(typeof(TRequest), new HandlerRegistration(typeof(TResult), bound)))
        {
            throw new InvalidOperationException($"A handler for {typeof(TRequest).FullName} is already registered.");
        }
    }

    internal async Task<TResult> DispatchAsync<TResult>(object request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestType = request.GetType();
        if (!registrations.TryGetValue(requestType, out var registration))
            throw new InvalidOperationException($"No handler is registered for {requestType.FullName}.");
        if (registration.ResultType != typeof(TResult))
            throw new InvalidOperationException($"The result type for {requestType.FullName} must be {registration.ResultType.FullName}, not {typeof(TResult).FullName}.");
        var invoke = (Func<object, CancellationToken, Task<TResult>>)registration.Invoke;
        var task = invoke(request, token);
        if (task is null)
            throw new InvalidOperationException($"The handler for {requestType.FullName} must return a non-null task.");
        var result = await task.ConfigureAwait(false);
        if (result is null)
            throw new InvalidOperationException($"The handler for {requestType.FullName} must return a non-null result.");
        return result;
    }
}
