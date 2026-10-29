# Web example

Run `dotnet run --project examples/Web -c Release -- --urls http://127.0.0.1:5080`.
The example accepts loopback addresses only. In another terminal:

```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:5080/demo/commands -ContentType application/json -Body '{"value":"example"}'
Invoke-RestMethod -Uri http://127.0.0.1:5080/demo/queries/example
```

Both return `value` and `scopeId`. Separate HTTP requests have distinct scoped
identities. Stop the host with Ctrl+C. This demonstrates registration, not generated
library endpoints or a production service. This example registers handlers as scoped interface services, then selects exact
CQRS mappings through those contracts. Helpers activate services lazily in the
request scope. Missing or ambiguous unkeyed handler services fail explicitly.
The alternative three-argument helpers own concrete scoped registration; combining
those helpers with external concrete registrations is a conflict.
The container owns scoped resources; use asynchronous scope disposal for async-only
resources. Applications provide handler thread safety, even within one scope.

Every top-level result must be non-null, even with nullable result types. Represent
absence explicitly, for example with a non-null `LookupResult(bool Found, string? Value)`
record. Null members are allowed. Null tasks, missing or duplicate mappings and wrong
result contracts fail explicitly. Handlers receive the original cancellation token;
their exceptions and cooperative cancellation propagate without a wrapper.


## Scoped worker and decorator

Run `dotnet run --project examples/Web -c Release -- --worker` for a finite Generic
Host demonstration. It executes two units of work and exits. Each unit creates an
asynchronous scope, resolves a scoped application operation and awaits its command
and query before disposing that scope. Their shared dependency has the same identity
inside a unit and a different identity in the next unit. Tests assert one async
disposal and one decorator invocation per unit.

`ScopedDispatchWorker` is the host boundary and receives `IServiceScopeFactory`.
`WorkerOperation` receives dispatcher interfaces through its constructor. It does
not resolve its own services. Never inject a scoped dispatcher into a singleton
worker or automatically manufacture a scope for every individual dispatch: several
operations may deliberately share one unit of work and its resources.

`WorkerCommandDecorator` receives its inner handler and scoped dependency normally;
the inner handler is an ordinary dependency, not an additional owning interface
alias to the selected disposable service. For one disposable implementation shared
across mappings, consistently use the three-argument concrete helpers, which register
that implementation once. Registering multiple container factories that return one
disposable instance may make the container dispose it more than once. SharpQrs
neither introduces those aliases nor takes ownership from the container.


## Checked construction

The web and worker hosts use `SharpQrsServiceProviderFactory` through their shared
`ExampleComposition`. The wrapper prepares the final descriptors after host container
configuration. For direct construction, build from
`services.PrepareSharpQrs().BuildServiceProvider(options)` instead. Interface mappings
require this boundary; copied/customised collections must be prepared independently.
See [composition and migration](../../docs/di-composition.md) for ordinary concrete
compatibility and the stronger final-validation guarantees.
