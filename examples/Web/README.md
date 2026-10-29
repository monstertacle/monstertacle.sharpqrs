# Web example

Run `dotnet run --project examples/Web -c Release -- --urls http://127.0.0.1:5080`.
The example accepts loopback addresses only. In another terminal:

```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:5080/demo/commands -ContentType application/json -Body '{"value":"example"}'
Invoke-RestMethod -Uri http://127.0.0.1:5080/demo/queries/example
```

Both return `value` and `scopeId`. Separate HTTP requests have distinct scoped
identities. Stop the host with Ctrl+C. This demonstrates registration, not generated
library endpoints or a production service. Helpers select exact handlers, activate
them lazily and register each concrete handler only once as scoped. External
registrations for selected concrete handlers or dispatchers are conflicts.
The container owns scoped resources; use asynchronous scope disposal for async-only
resources. Applications provide handler thread safety, even within one scope.

Every top-level result must be non-null, even with nullable result types. Represent
absence explicitly, for example with a non-null `LookupResult(bool Found, string? Value)`
record. Null members are allowed. Null tasks, missing or duplicate mappings and wrong
result contracts fail explicitly. Handlers receive the original cancellation token;
their exceptions and cooperative cancellation propagate without a wrapper.
