# Checked DI composition and migration

Interface-based mappings now validate the same final descriptor set used to build
their provider. Finish service registration and decoration before choosing mappings,
then prepare each actual composition immediately before building it. Configuration
is setup-only; do not mutate a collection concurrently with preparation.

## Direct construction

```csharp
services.AddScoped<ICommandHandler<CreateOrder, OrderResult>, CreateOrderHandler>();
services.AddSharpQrsCommand<CreateOrder, OrderResult>();
await using var provider = services.PrepareSharpQrs().BuildServiceProvider(
    new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
await using var scope = provider.CreateAsyncScope();
var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
var result = await dispatcher.DispatchAsync(command, cancellationToken);
```

The application defines the request, result and handler types. Query and synchronous
contract mappings use the same preparation step. Exactly one unkeyed service must
match each selected contract. Keyed services do not count. Handlers stay lazy, and
container scope disposal retains ownership. Transient selections are resolved per
invocation; no adapter caches a handler or manufactures a new scope.

Preparation validates without constructing providers or activating handlers. It
returns a separate read-only collection and leaves its input unchanged, including
on failure. Build from the returned collection. If you copy descriptors and change
the copy, prepare that copy; do not copy old prepared evidence and bypass the final
boundary. Calling preparation again replaces that evidence. Collections and scopes
are not live configuration mechanisms.

## Host construction

```csharp
builder.Host.UseServiceProviderFactory(new SharpQrsServiceProviderFactory(
    new DefaultServiceProviderFactory(new ServiceProviderOptions
    {
        ValidateScopes = true,
        ValidateOnBuild = true
    })));
```

For callback-based Generic Host, call the equivalent `UseServiceProviderFactory`
on the host builder. `CreateBuilder` delegates normally, permitting final container
configuration; the wrapper prepares that final builder immediately before calling
the inner factory's `CreateServiceProvider`. The inner factory must accept a read-only
collection and must not add registrations during provider creation. The wrapper
adds no DI implementation package or per-dispatch scope.

## Existing consumers

The published 0.1.0-alpha.1 asynchronous interfaces and three-argument concrete
helpers retain their signatures and ordinary provider-construction path. Their
setup-time conflict checks remain. Preparation additionally rejects later duplicate,
removed or replaced owned concrete/dispatcher descriptors before construction.

Consumers of feature 002's unreleased two-argument interface mapping helpers must
add the preparation call or host wrapper. The owner explicitly approved this setup
contract change. An unprepared interface dispatcher throws a diagnostic naming both
migration options, rather than inspecting the collection from some other composition.
Existing cancellation, result, exception, ownership and routing contracts remain.
No new NuGet release is implied by this migration document.

## Ownership and practical composition

For ordinary operations, select one handler-interface service and inject dispatcher
interfaces into scoped application services. Apply decorators before selecting the
mapping. Register inner dependencies with their own clear container ownership.
Do not add several owning interface factories that return the same disposable
instance: the container may dispose each registered service. For a single disposable
concrete handler shared across mappings, consistently use concrete helpers, which
register it once.

A singleton worker receives `IServiceScopeFactory`. For each unit of work, it creates
an asynchronous scope, resolves a scoped application operation and awaits completion
before disposing the scope. That application operation receives dispatchers through
constructor injection. Related commands/queries can share one unit's scoped state;
automatic scopes per dispatch would lose that sharing.

Run `dotnet run --project examples/Web -c Release -- --worker` to execute the finite
worker/decorator demonstration. Integration tests assert distinct unit scopes, shared
command/query identities inside each unit, cancellation and original errors, and
one asynchronous disposal per unit. These examples are measured by the same coverage
and static metrics checks as the libraries and existing examples.
