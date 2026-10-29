# Monstertacle.SharpQrs

Typed, in-process command and query dispatch for .NET 10, with a separate scoped
registration integration. Both libraries are independent of SharpClaw.

- `Monstertacle.SharpQrs`: dependency-free core with explicit handler contracts and synchronous or asynchronous dispatch.
- `Monstertacle.SharpQrs.AspNetCore`: explicit DI registration and scope-aware lazy
  handler resolution; depends only on the core and DI abstractions.

## Build and verify

Install .NET SDK **10.0.403** and **PowerShell 7**, then run:

```powershell
dotnet tool restore
pwsh -NoProfile -File scripts/verify.ps1
```

Verification uses locked restore, warnings-as-errors Release builds, behavioural
tests, at least 90% line coverage **in each library**, complete Codemetr coverage
and diff checks, published-consumer compatibility, the strict full-source
Codemetr guard, a dependency vulnerability audit, runnable examples and isolated
NuGet package consumers. Reports are written to a fresh ignored `artifacts/verify-*`
directory. Windows and Linux CI run the same script. Publishing uses the separate
[manual NuGet release workflow](.github/RELEASING.md).

## Standalone usage

```csharp
var dispatcher = new CommandDispatcher();
dispatcher.Register<MyCommand, MyResult>(new MyCommandHandler());
MyResult result = await dispatcher.DispatchAsync(new MyCommand(), cancellationToken);
```

Implement `ICommand<MyResult>` and `ICommandHandler<MyCommand, MyResult>`, whose
`HandleAsync` returns `Task<MyResult>`. Queries use `IQuery<TResult>`,
`IQueryHandler<TQuery, TResult>` and `QueryDispatcher` in the same way.
See the [complete standalone example](examples/Standalone/README.md).

## Scoped integration

```csharp
services.AddSharpQrsCommand<MyCommand, MyResult, MyCommandHandler>();
services.AddSharpQrsQuery<MyQuery, MyResult, MyQueryHandler>();
```

These helpers also register infrastructure; `services.AddSharpQrs()` alone is
idempotent. Resolve `ICommandDispatcher` and `IQueryDispatcher` inside an application
or request scope. Enable DI scope validation. Concrete handlers are registered
once as scoped, even when selected for several distinct contracts. They activate
only when needed; container scope disposal owns their resources. Use asynchronous
scope disposal for async-only services. Do not separately register selected concrete
handlers or dispatcher services. Helpers reject pre-existing conflicts before
mutating the collection; adding or replacing concrete registrations afterwards is
unsupported. Ordinary container construction does not prevent later descriptors
from overriding earlier registrations.
Existing handler-interface services can be selected explicitly instead:

```csharp
services.AddScoped<ICommandHandler<MyCommand, MyResult>, MyCommandHandler>();
services.AddSharpQrsCommand<MyCommand, MyResult>();
await using var provider = services.PrepareSharpQrs().BuildServiceProvider(
    new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
```

This selects exactly one unkeyed handler service without activating it. Decorators
and existing service lifetimes are preserved. Missing or ambiguous services fail
explicitly. Interface mappings require checked construction: register services and
mappings, finish configuration, then build from `PrepareSharpQrs()`'s returned
read-only copy. Preparation checks the actual final descriptors without activating
handlers and leaves the original collection unchanged. Copied/customised collections
must be prepared again immediately before building; copied old preparation evidence
is replaced. Building interface mappings without preparation fails with migration
instructions when their dispatcher is resolved. Query handlers have the corresponding two-argument helper. There is
no implicit discovery. Standalone setup can use `ICommandRegistration` and
`IQueryRegistration`, leaving runtime consumers dependent on dispatch alone.
See the [web example](examples/Web/README.md).

For Generic Host or ASP.NET Core, use the wrapper so validation runs after final
container configuration:

```csharp
builder.Host.UseServiceProviderFactory(new SharpQrsServiceProviderFactory(
    new DefaultServiceProviderFactory(new ServiceProviderOptions
    {
        ValidateScopes = true,
        ValidateOnBuild = true
    })));
```

The host supplies the inner factory; the integration still depends only on DI
abstractions. It must accept a read-only service collection during provider creation.
Existing published concrete-only registrations continue to support ordinary
`BuildServiceProvider`. Use preparation for complete final validation of those
registrations too: late additions, replacement or removal of owned concrete or
dispatcher descriptors are then rejected. Setup-time checks alone cannot stop later
container overrides. See [migration and composition guidance](docs/di-composition.md).

## Synchronous usage

```csharp
var dispatcher = new SyncCommandDispatcher();
ISyncCommandRegistration registrations = dispatcher;
registrations.Register<MyCommand, MyResult>(new MySyncCommandHandler());
ISyncCommandDispatcher commands = dispatcher;
MyResult result = commands.Dispatch(new MyCommand(), cancellationToken);
```

Implement `ISyncCommandHandler<MyCommand, MyResult>.Handle` returning `MyResult`
directly. Queries use `ISyncQueryHandler`, `ISyncQueryRegistration`,
`ISyncQueryDispatcher` and `SyncQueryDispatcher`. Synchronous work can cooperate
with cancellation; tokens are forwarded unchanged.

Scoped composition offers either concrete selection or an existing service contract:

```csharp
services.AddSharpQrsSyncCommand<MyCommand, MyResult, MySyncCommandHandler>();
// Alternatively, register a service and select its contract:
services.AddScoped<ISyncQueryHandler<MyQuery, MyResult>, MySyncQueryHandler>();
services.AddSharpQrsSyncQuery<MyQuery, MyResult>();
```

These helpers install synchronous infrastructure automatically. For empty dispatchers,
call `AddSharpQrsSynchronous()`. Resolve dispatchers inside a scope. Sync and async
mappings are independent, even for the same request type. Wrong-mode dispatch fails;
there is no task blocking, implicit adaptation or background scheduling. Await
`DispatchAsync` for asynchronous handlers and call `Dispatch` for synchronous ones.

Concrete handler types belong at the composition boundary so the container can
construct them. Domain consumers need only dispatcher interfaces. Additional
interfaces here separate setup, execution mode and command/query responsibilities;
adding an interface to every implementation would not itself establish SOLID design.

## Contracts

Registration is completed before dispatch. Routing matches the exact concrete
request type and result type, without base-type fallback, scanning or reflection-based
invocation. Duplicate registration fails without replacing an original handler.
Each call invokes its handler once. Overlapping calls route independently; applications
remain responsible for handler thread safety. Standalone handlers remain caller-owned.

Null requests, handlers and service collections are rejected explicitly. Missing
mappings, wrong result contracts, null tasks and **every actual null result** fail
with a diagnostic. Nullable members inside a non-null result are permitted. Represent
absence with an application-defined non-null value, such as a record containing
`Found` and nullable data. No mandatory result wrapper is imposed.

Cancellation tokens, including already-cancelled tokens, reach handlers unchanged.
Handlers control cooperative cancellation. Original synchronous/asynchronous
exceptions and cancellation propagate without dispatcher-added wrappers.
No retries, transactions, persistence, distributed messaging or generated HTTP
endpoints are provided.

## Development

See [development and quality gates](docs/development.md), the
[quickstart validation guide](specs/001-cqrs-libraries/quickstart.md),
[public contracts](specs/001-cqrs-libraries/contracts/core-api.md) and
[implementation evidence](specs/001-cqrs-libraries/implementation-evidence.md).
The initial package version is `0.1.0-alpha.1`; this repository does not claim
compatibility with another library's API. Licensed under [MIT](LICENSE).
