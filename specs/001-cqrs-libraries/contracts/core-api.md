# Core Library Contract

**Assembly / package / namespace**: `Monstertacle.SharpQrs`
**Target**: `net10.0` | **Dependencies**: BCL only | **Licence**: MIT

These are proposed public signatures, not implementation source. Each declared type
will have its own file. No public registry base, container abstraction or result-wrapper
type is required.

## Request Markers

```csharp
public interface ICommand<TResult> { }
public interface IQuery<TResult> { }
```

These are empty invariant interfaces. Command and query categories are distinct typing conventions,
not guarantees of side-effect freedom or security.

## Handler Interfaces

```csharp
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

`TResult` has no `notnull` constraint: all null results are rejected at runtime,
including types declared nullable. Non-null nullable value-type results are valid.
Application handlers represent absence with an explicit non-null value of their choice.
No result wrapper is imposed by the library.

## Dispatch Interfaces

```csharp
public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TResult>(
        ICommand<TResult> command, CancellationToken cancellationToken = default);
}

public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TResult>(
        IQuery<TResult> query, CancellationToken cancellationToken = default);
}
```

Both permit result inference from the request interface. Cancellation is optional
at the call site, but the actual supplied token must reach the handler unchanged.

## Concrete Standalone Dispatchers

| Public type | Construction | Registration | Execution |
| --- | --- | --- | --- |
| Sealed `CommandDispatcher` implementing `ICommandDispatcher` | Public parameterless constructor | `void Register<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler)` where `TCommand : ICommand<TResult>` | Same `DispatchAsync` contract as its interface |
| Sealed `QueryDispatcher` implementing `IQueryDispatcher` | Public parameterless constructor | `void Register<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)` where `TQuery : IQuery<TResult>` | Same `DispatchAsync` contract as its interface |

No public unregistration, replacement, fallback, automatic scanning or live
reconfiguration API. Setup must complete before dispatch; no concurrent mutation
guarantee is made. Independent dispatcher instances have independent registrations.

## Dispatch Semantics and Errors

Checks occur in this order for a supported call:

1. Reject null request with `ArgumentNullException`.
2. Look up `request.GetType()` in the corresponding category. Missing registration
   fails with `InvalidOperationException` identifying its full request type.
3. Require the registered result CLR type to equal the requested result CLR type.
   A mismatch fails with `InvalidOperationException` before invoking the handler.
4. Invoke the selected handler once with the original request and token.
5. Reject a null returned task with `InvalidOperationException` identifying the request.
6. Await the task without blocking. Handler exceptions and cancellation propagate
   without a dispatcher-added wrapper. Error observation may occur when awaiting
   `DispatchAsync`; synchronous versus faulted-task timing is not an API guarantee.
7. Reject any null result with `InvalidOperationException` identifying the request
   and non-null requirement. Return a valid result; do not inspect nullable members.

Null handler registration fails with `ArgumentNullException`. Duplicate request
registration fails with `InvalidOperationException` before changing the existing entry.
Diagnostics use British English and full type identities; tests assert exception type,
relevant identities and semantic meaning, not punctuation or whole-message wording.

## Ownership and Concurrency

Dispatchers do not implement disposal or own handlers. They retain registered handler
references; callers ensure those handlers remain usable for the dispatcher's intended
lifetime and dispose their own resources afterwards.

Overlapping dispatches after setup must route independently without shared per-call
state. A handler may receive simultaneous calls; the application must provide appropriate
thread safety. No global mutable registry, automatic retries or query-effect checks.

## Acceptance Mapping

FR-002–FR-007 and FR-013–FR-015 are covered by both request-category test families.
Include a request implementing two distinct result contracts to test exact result
identity; null task and null result probes intentionally violate handler annotations
at runtime without blanket warning suppression. Cover reference and nullable value
results, synchronous and delayed failures, token/exception identity and duplicate
registration preservation.
