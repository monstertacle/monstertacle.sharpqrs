# Data Model: General-purpose CQRS Libraries

**Feature**: `001-cqrs-libraries` | **Date**: 2026-10-28

This is an in-memory contract and registration model, not a persistence schema.
No database, serialisation format or durable identity is introduced.

## Request Contracts

### Command and Query

- **Identity**: Concrete CLR request type, in the command or query category.
- **Relationship**: Implements `ICommand<TResult>` or `IQuery<TResult>`.
- **Payload**: Application-defined; never inspected by dispatch infrastructure.
- **Result**: Exact CLR `TResult` recorded at registration; every actual null result
  is prohibited, including an empty nullable value type.
- **Validation**: Request instance must not be null. The dispatcher does not perform
  payload validation, authorisation or query side-effect analysis.

The marker interfaces are invariant. A deliberately multi-contract request can expose
different result interfaces, but a dispatcher still admits only one result mapping
for its concrete request type. Dispatch through a conflicting result interface fails
before handler execution. Derived request types require their own registrations.

## Handler

- **Identity**: Caller-provided instance in core usage; registered concrete CLR type
  in integrated usage.
- **Relationship**: Implements the appropriate request/result handler contract.
- **Operation**: Accepts the typed request and cancellation token, returns `Task<TResult>`.
- **Multiplicity**: A concrete handler may implement several distinct contracts.
- **Ownership**: Caller-owned in core usage; container-owned for helper-created scoped
  handlers in integrated usage. Dispatchers and adapters never dispose handlers.
- **Concurrency**: Application responsibility for shared handler state; dispatchers do
  not serialise calls or claim that handlers are thread-safe.

No task or handler instance is stored in global mutable state.

## Core Registration

| Attribute | Meaning |
| --- | --- |
| Request type | Exact request lookup key within one dispatcher |
| Result type | Exact expected CLR result type |
| Invocation delegate | Bound typed handler call, adapted to the internal registry |

**Constraints**: Handler instance is non-null. The request type key must be unique
within that dispatcher's category. Attempting a duplicate must leave the original
entry unchanged. Registration precedes execution; concurrent mutation is unsupported.

**Lifetime**: Registration retains a reference to the supplied handler until the
dispatcher becomes unreachable. It does not transfer ownership or create a lease.

## Integration Registration Metadata

| Attribute | Meaning |
| --- | --- |
| Request category | Command or query |
| Request type | Exact concrete request type |
| Result type | Expected CLR result type |
| Concrete handler type | Scoped service to resolve when invoked |
| Typed adapter registration action | Installs the appropriate non-owning adapter |

Metadata is immutable after service setup. It captures no provider or handler.
Every request/category pair is unique, including registrations using the same
concrete handler twice. Concrete handler types are registered only once even when
selected for multiple distinct contracts. Registrations belong to one service
collection and resulting provider, not to a static process-wide registry.

## Scoped Adapter and Dispatcher

- One command dispatcher and one query dispatcher are resolved per DI scope.
- Each adapter holds the current scope's provider and a typed handler contract,
  not a root provider or a newly created scope.
- The concrete handler is resolved only on invocation. Repeated resolution within
  the scope uses the same scoped concrete instance.
- Adapter calls return the original handler task without an asynchronous wrapper,
  so a malformed null task remains detectable by the core's task validation.
- Missing dependencies fail through normal container resolution. No fallback handler,
  retry, private provider or provider-construction call is allowed.

## Lifecycle and State Transitions

1. **Setup**: Create a standalone dispatcher or add integration registrations.
   Reject invalid inputs and duplicate mappings before applying that registration.
2. **Ready**: Core callers finish registration; integrated callers build the host or
   provider only after service setup. The library itself never builds a provider.
3. **Dispatching**: Validate request and mapping, invoke once, validate task, await,
   reject null result, then return a valid result. Failure ends only this invocation.
4. **Scope completed**: Container-owned handlers/dependencies are disposed by the
   scope, including async-only disposal through an asynchronously disposed scope.
   Caller-owned core handlers are disposed by their caller when appropriate.

Do not reuse a scope-bound dispatcher after its scope ends; the container governs
subsequent resolution failures. No explicit freeze API, request history or stored
execution state is required. Read-only dispatch maps support overlapping invocations.

## Null, Failure and Cancellation Rules

- Invalid caller inputs: `ArgumentNullException` for null request, handler or service
  collection; normal generic constraints rule out incompatible handler selections.
- Missing/duplicate mappings, wrong result identity, null task and null result:
  `InvalidOperationException`, with the request identity where applicable.
- Original synchronous or asynchronous handler exceptions propagate unchanged.
- An already-cancelled token is forwarded; no pre-emptive cancellation check suppresses
  the handler invocation. Cooperative cancellation is not converted into a generic error.
- Null checks apply to the top-level result, not recursively to its members.

See the [core contract](contracts/core-api.md) and
[integration contract](contracts/registration-api.md) for the public surface.
