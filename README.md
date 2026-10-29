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
```

This selects exactly one unkeyed handler service without activating it. Decorators
and existing service lifetimes are preserved. Missing or ambiguous services fail
explicitly; selection is validated during setup and scoped dispatcher construction.
Register services before selecting mappings and finish configuration before building
the provider. Query handlers have the corresponding two-argument helper. There is
no implicit discovery. Standalone setup can use `ICommandRegistration` and
`IQueryRegistration`, leaving runtime consumers dependent on dispatch alone.
See the [web example](examples/Web/README.md).

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
