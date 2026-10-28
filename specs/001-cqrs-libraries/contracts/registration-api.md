# ASP.NET Core Registration Contract

**Assembly / package / namespace**: `Monstertacle.SharpQrs.AspNetCore`
**Target**: `net10.0` | **Licence**: MIT
**Dependencies**: Core library and `Microsoft.Extensions.DependencyInjection.Abstractions`
10.0.14. No production hosting, HTTP or concrete-container dependency.

## Public Surface

A public static `SharpQrsServiceCollectionExtensions` class exposes:

```csharp
IServiceCollection AddSharpQrs(this IServiceCollection services);

IServiceCollection AddSharpQrsCommand<TCommand, TResult, THandler>(
    this IServiceCollection services)
    where TCommand : ICommand<TResult>
    where THandler : class, ICommandHandler<TCommand, TResult>;

IServiceCollection AddSharpQrsQuery<TQuery, TResult, THandler>(
    this IServiceCollection services)
    where TQuery : IQuery<TResult>
    where THandler : class, IQueryHandler<TQuery, TResult>;
```

These method signatures are contract notation; implementations will be public static
extension methods. `THandler` must be a concrete, closed implementation type; reject
abstract or open types during registration. A null service collection is rejected
with `ArgumentNullException`. Each method returns the original collection for chaining.

Handler helpers ensure infrastructure registration, so an explicit `AddSharpQrs`
call is optional. No options object, builder hierarchy, lifetime selector or assembly
scanner is introduced in this version.

## Registration and Duplicate Rules

- `AddSharpQrs` adds scoped `ICommandDispatcher` and `IQueryDispatcher` services and
  required internal factories once. Repeating it does not add descriptors or metadata.
- The handler helpers register an exact request/result mapping and a scoped concrete
  handler service. A second mapping for the same category and concrete request type
  always fails with `InvalidOperationException`, even if identical. Failed validation
  must not partly add the new mapping or its concrete handler.
- One concrete handler can serve multiple distinct mappings but is registered and
  resolved only once per scope. Internal tracking identifies helper-owned concrete
  registrations; it contains no mutable global state.
- Pre-existing external registrations for the selected concrete handler are rejected
  clearly rather than adopted, replaced or assigned another lifetime. Register that
  handler through these helpers and register its dependencies normally.
- Pre-existing external dispatcher registrations are configuration conflicts and
  are rejected, not silently replaced. Repeated helper-owned registrations remain
  idempotent for infrastructure only.
- Direct registrations of handler interfaces are not scanned or adopted. Only
  helper-selected mappings participate. Manual removal or mutation of internal
  descriptors after helper calls is unsupported configuration tampering.
- Provider construction belongs to the host or consumer. Helpers never call
  `BuildServiceProvider`, construct a temporary provider or resolve a scoped handler.

## Scope, Activation and Disposal

- Factories create one dispatcher of each category per scope using that scope's
  provider and immutable registration metadata. They validate metadata uniqueness
  before installing adapters or executing handlers.
- Each non-disposable adapter resolves its selected concrete handler only when
  invoked, returns its original `Task<TResult>` and performs no handler disposal.
- No service descriptor aliases a disposable concrete handler under several handler
  interfaces. This prevents duplicate disposal of a multi-contract handler instance.
- Scoped dependencies are shared within their scope and remain distinct across scopes.
  Unselected handlers and handlers for unrelated requests are not activated by dispatch.
- Container-owned handlers and dependencies are released by their scope exactly once.
  Async-only resources require asynchronous scope disposal. Neither dispatcher nor
  adapter calls disposal itself.
- Consumers must resolve these services within a request or explicitly created scope.
  Examples and tests enable scope validation. The library does not override host
  provider validation settings or manufacture scopes to conceal incorrect root usage.

No promise is made that a scoped handler is safe for concurrent use. Application
authors remain responsible for handler state even within one shared request scope.

## Dispatch Behaviour

The [core contract](core-api.md) applies unchanged, including exact request/result
matching, original token forwarding, invalid-task detection and universal null-result
rejection. Normal DI dependency-resolution failures propagate; no fallback to a root
provider, another scope or another handler is permitted.

## Acceptance Mapping

- FR-008–FR-010: actual service registration, scope-validation, idempotence, selection,
  conflict and dependency-failure tests.
- FR-005: duplicate mapping rejected before handler execution and without partial
  registration changes; repeat the check for identical handler selections.
- FR-009 and FR-014: sync/async disposal counters, including one concrete handler
  implementing multiple distinct command/query contracts.
- FR-015: two independently configured service providers use their own mappings.
- SC-002: TestHost requests use real request scopes; verify identity and eventual
  exactly-once disposal after each completed request, using deterministic signals.
- SC-003: repeat relevant core failure and cancellation probes through integrated
  dispatch rather than assuming delegation makes their tests unnecessary.
