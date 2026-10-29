# Standalone example

Run `dotnet run --project examples/Standalone -c Release` from the repository root.
The example references only the core and verifies synchronous and asynchronous commands and queries.
Setup uses narrow registration interfaces; runtime calls use the matching dispatch
mode. Synchronous calls return direct results and never block on asynchronous tasks.
Register all handlers before dispatch. Handlers remain caller-owned; dispose any
resources you supply after all dispatches finish. Overlapping dispatch is supported,
but applications must make shared handlers thread-safe.

Every top-level result must be non-null, even with nullable result types. Represent
absence explicitly, for example with a non-null `LookupResult(bool Found, string? Value)`
record. Null members are allowed. Null tasks, missing or duplicate mappings and wrong
result contracts fail explicitly. Handlers receive the original cancellation token;
their exceptions and cooperative cancellation propagate without a wrapper.
