# Quickstart validation guide

Feature: `001-cqrs-libraries`. Use a fresh checkout with .NET SDK **10.0.403**,
PowerShell **7**, Git and network access to nuget.org. No SharpClaw checkout,
external source tree or globally installed metrics tool is needed.

## Complete verification

From the repository root:

```powershell
dotnet --version
dotnet tool restore
pwsh -NoProfile -File scripts/verify.ps1
```

Expected: SDK 10.0.403, restored local Codemetr 0.1.0 and all automated gates pass.
The entry point performs locked restore, warning-free Release build, actual VSTest
execution, fresh per-library coverage, full-source metrics, dependency audit,
standalone/web smoke checks and isolated NuGet package consumers. It also exercises
the checker harnesses. Neither audit nor packaging is skipped.
On the shared development machine, wrap the entry point with
`bash ~/dev/scripts/run-capped.sh -- pwsh -NoProfile -File scripts/verify.ps1`.

Reports identify source revision, dirty-file status, tool versions and run times in
a unique ignored `artifacts/verify-*` directory. Both production modules must have
at least 90% line coverage; branch totals require review without an extra numerical
gate. Missing, malformed, stale or incomplete evidence fails verification.
Codemetr's synthetic compilation does not resolve all external symbols; inspect
advisory metrics and the separate real build rather than treating exit zero as
proof of SOLID or complete coupling analysis.

The dependency audit checks all solution projects/frameworks against committed
lock files and nuget.org's vulnerability source. Every reported vulnerability
fails. A clean result means no known vulnerabilities were reported for this graph;
it is not a complete SDK/tool-binary security assessment. See the
[development guide](../../docs/development.md) for classification and limitations.

## Standalone demonstration

```powershell
dotnet run --project examples/Standalone --configuration Release --no-build --no-restore
```

Expected: a command and query return the correct values, each invoking its handler
once. The example references only the BCL-only core; caller ownership is retained.
The full verification already executes it and an equivalent package-only consumer
outside the source tree. The standalone runtime requires no ASP.NET Core runtime.

## Web demonstration

```powershell
dotnet run --project examples/Web --configuration Release --no-build --no-restore -- --urls http://127.0.0.1:5080
```

In another terminal:

```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:5080/demo/commands -ContentType application/json -Body '{"value":"example"}'
Invoke-RestMethod -Uri http://127.0.0.1:5080/demo/queries/example
```

Expected: `value` is `example`; each request returns a distinct `scopeId`.
Stop the host with Ctrl+C. It accepts loopback binding only. These are hand-written
example routes, not a production HTTP contract or library-generated endpoints.
The verification entry point starts its own host on an ephemeral loopback port,
executes both calls, checks results/scope identities and stops its owned process.
Actual TestHost tests separately prove same-request reuse and exactly-once disposal.

## Contracts and acceptance

Tests cover both categories and usage modes: exact request/result identity, null
requests/handlers/tasks/results, missing and duplicate registrations, atomic conflict
rejection, original exception/token propagation, cancellation, concurrent routing,
provider isolation and caller/container ownership. Non-null absence values are
application-defined; nullable members inside a non-null result remain valid.

The packages use the prerelease version `0.1.0-alpha.1`, include MIT notices and
are consumed through a temporary local feed during verification. Nothing is published.
For actual outcomes, hosted platform results, TDD traceability, advisory review and
the fresh-checkout source revision, see [implementation evidence](implementation-evidence.md).
