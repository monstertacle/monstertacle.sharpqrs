# SharpQrs architecture and DI review

Keep the dependency-free CQRS core and explicit execution contracts. Correct two
DI composition problems before the next feature release, and make the recommended
registration and ownership patterns easier to follow. Additional interfaces,
automatic discovery and an execution pipeline do not address the observed problems.

The review concerns source revision `9216f275256afcd70c6dec44cf9651d7cf12f837`.
Two independent software architects received the same frozen source and measured
baseline with opposing briefs. One argued for changes; the other defended the
existing design. Both had to state weaknesses in their own position. The
[measured evidence](2026-10-29-cqrs-di-evidence.json) records their timing, call
accounting and the independently executed composition probes.

## Opposing assessments

| Question | Architect advocating change | Architect defending the design | Final decision |
| --- | --- | --- | --- |
| Does the core need redesign? | No; focus on developer composition friction | No; preserve exact routing and small contracts | Retain core design |
| Should sync registration inspect async infrastructure? | No; overloads behave inconsistently | No; a genuine accidental dependency | Correct DI-001 |
| Can retained collection state validate copied compositions correctly? | No; settle the intended composition boundary carefully | No; changes before building any provider are not excused by the after-build restriction | Correct DI-002 through a planned boundary decision |
| Does later concrete replacement prove supported dispatch is broken? | No; it violates instructions but exposes an enforcement mismatch | No; the rejection claim is too broad | Resolve UX-001 explicitly |
| Are repeated disposal calls caused by the adapters? | No; applications created multiple owning aliases | No; the container owns each registered service | Add safe composition guidance under UX-002 |
| Are more abstractions or performance changes justified? | No workload or consumer requirement supports them | They risk weakening simple contracts and transient semantics | Defer speculative expansion |

The change advocate recognised that ordinary applications following registration
order already work, and that copying descriptors is an integration edge case. The
defending architect conceded that strong coverage does not establish correctness
for every composition pattern. Both accepted the two focused corrections after
inspecting the same additional probes.

## Confirmed DI findings

### Synchronous concrete registration checks the other execution mode

**DI-001, P2.** Registering an external `ICommandDispatcher` and then calling
`AddSharpQrsSyncCommand<Request, int, Handler>()` fails with an asynchronous
infrastructure conflict. Registering the same synchronous handler through its
interface and using the two-argument helper succeeds and returns `42`.

[SyncRegistrationValidation.cs](../../src/Monstertacle.SharpQrs.AspNetCore/SyncRegistrationValidation.cs#L28)
calls `RegistrationValidation.Handler`, which first validates the asynchronous
dispatchers. Handler-type/ownership validation should be reusable independently
of mode-specific infrastructure validation. This preserves shared concrete-handler
ownership while allowing independent modules and substituted async dispatchers.

### Copied service collections are validated against the original

**DI-002, P2.** The installation closures in
[SharpQrsContractExtensions.cs](../../src/Monstertacle.SharpQrs.AspNetCore/SharpQrsContractExtensions.cs#L20)
and the synchronous counterparts retain the original `IServiceCollection`, while
their adapters resolve from the provider supplied at installation.

The probe first registers a handler and mapping, then copies all descriptors into
a second collection. Adding another matching handler to the copy before building
its provider silently selects the second handler and returns `2`, although the
mapping promises exactly one service. In a separate case, removing the handler
from the original collection before building the unchanged copy makes that valid
copy fail with a missing-service diagnostic. Neither case mutates a collection
after provider construction.

Validation and resolution need to describe the same effective composition. A fix
must preserve rejection of late ambiguity and lazy handler activation. A mutable
collection copied at mapping time would merely move the mismatch. Resolving all
handlers to count them would activate services during validation. An explicit
validation/finalisation boundary or provider-factory integration therefore needs
assessment before choosing a compatible implementation. The current workaround
is to apply registration independently to each collection instead of copying
mapping descriptors between customised compositions.

### Concrete replacement is outside the supported pattern but rejection is incomplete

**UX-001, P3.** The concrete helper rejects an existing external concrete descriptor,
but adding another concrete descriptor after the helper and before provider
construction dispatches the replacement and returns `99`. The README prohibits
this setup, so it is not a supported replacement feature. Its accompanying claim
that these configurations are rejected atomically is broader than the actual
registration-order checks. Specify and test the final validation guarantee, or
state its narrower timing precisely.

### Shared disposable aliases need an ownership recipe

**UX-002, P3.** Registering one scoped disposable concrete handler and then two
owning interface factories that return that same object causes three `Dispose`
calls when all three services are resolved. Application registrations introduce
these aliases; the SharpQrs adapters do not own or dispose them. This is a consumer
composition hazard, not a dispatcher disposal defect. Changing adapters to dispose
handlers would worsen ownership. Microsoft's
[DI guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
assign disposal to the creating container and recommend explicit scopes.

## Recommended usage

For a normal scoped operation, register the handler service, decorate it if needed,
then select its exact contract. Inject the matching dispatcher into the application
service; handlers receive their own dependencies through constructors.

```csharp
services.AddScoped<ICommandHandler<CreateOrder, OrderResult>, CreateOrderHandler>();
services.AddSharpQrsCommand<CreateOrder, OrderResult>();
```

This is a composition fragment using application-defined types. Await
`ICommandDispatcher.DispatchAsync(command, cancellationToken)` for I/O work. Use
`ISyncCommandHandler` with `AddSharpQrsSyncCommand` and `ISyncCommandDispatcher.Dispatch`
for genuinely synchronous work. Cancellation remains cooperative in either mode.

| Consumer need | Recommended composition |
| --- | --- |
| One handler with normal scoped dependencies | One handler-interface descriptor, followed by its exact mapping |
| Decorators | One selected outer handler-interface descriptor; container-owned inner services with distinct ownership |
| One disposable implementation shared across several mappings | Consistently use concrete helpers, which register the concrete service once; avoid multiple owning interface aliases |
| Web request | Inject a dispatcher into a scoped application consumer and use the request scope |
| Singleton or background worker | Inject `IServiceScopeFactory`; create an asynchronous scope per unit of work, resolve the scoped application service there, await it and dispose the scope |
| Tests replacing a handler | Replace/decorate the selected interface service before mapping; rebuild the composition independently |
| Independent sync and async modules | Preserve separate mode contracts; use the interface-based sync helper as the current DI-001 workaround |

Enable `ValidateScopes` and, where appropriate, `ValidateOnBuild` in direct provider
construction. These do not execute every factory-backed lazy dependency path;
integration tests should exercise the selected operation. Microsoft's
[service lifetime guidance](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/service-lifetimes)
explains why a singleton must create a scope for scoped work. Do not create a new
scope automatically for each dispatch: several commands may intentionally share
one transaction and its scoped dependencies.

## Retained architecture and measured confidence

Keep explicit request/result mapping, exact concrete routing, narrow consumer
interfaces, separate synchronous contracts, unchanged exception/token propagation,
non-null top-level results and non-owning adapters. No allocation or throughput
benchmark establishes a performance defect. Caching a resolved handler could change
the tested transient-service behaviour.

The clean frozen checkout passed the complete verifier: 139 tests, 100% line
coverage in each library, complete coverage measurement across 53 library/example
files, strict metrics across 110 handwritten C# files, audit, examples, isolated
packages and published-consumer compatibility. Static cycle analysis is limited
to its selected source graph and unresolved external symbols. These results support
the baseline but do not invalidate the reproduced composition defects.

Every open finding has an owner, next steps and acceptance criteria in the
[DI composition follow-up plan](../plans/di-composition-hardening.md). This review
changes documentation only; it does not fix product code or publish packages.
