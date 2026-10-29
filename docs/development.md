# Development

Use .NET SDK 10.0.403, C# 14 and PowerShell 7. The SDK pin disables roll-forward
and prereleases. All five projects target `net10.0`; central package versions
and each project's lock file are committed. The test project uses xUnit v2 with
VSTest and Coverlet collector, not Microsoft.Testing.Platform.
Package pruning is disabled only in the test project to retain the explicitly
planned DI 10.0.14 dependency alongside the shared framework without NU1510.

Run `pwsh -NoProfile -File scripts/verify.ps1`. On the shared development machine,
use `bash ~/dev/scripts/run-capped.sh -- pwsh -NoProfile -File scripts/verify.ps1`.
Temporary package consumers live under `~/dev/.agenttemp`; only each invocation's
unique directory is removed. No source or packages are uploaded by verification.

## Test-driven changes

Follow Red -> Green -> Refactor. A compilable stub may establish signatures, but
an executed behavioural assertion must fail before new behaviour is implemented.
An already-passing test is recorded as a regression, never a fabricated Red.
Record commands and results in the feature's implementation evidence. Each type
has its own file; tests use deterministic completion signals rather than sleeps.
Actual HTTP request-scope tests use TestHost; container tests enable scope validation.

The coverage checker requires both production modules with executable lines and
at least 90% line coverage separately. It rejects missing, malformed, empty, stale
or below-threshold reports. Branch totals are reported and reviewed; there is no
separate numerical branch threshold. No hand-written production code is excluded.

## Metrics and review

The repository-local manifest pins published `Codemetr.CSharp.Tool` 0.1.0. Restore
it with `dotnet tool restore`. The approved `codemetr.json` gates all hand-written
C# in `src`, `tests` and `examples`, with matching context and aggregated partial
types. No baselines, relaxed overrides or hand-written exclusions are allowed.
The ten limits and their design rationale are in the implementation plan.

`check-metrics.ps1` preserves failing tool exits and reconciles measured source paths
against the actual inventory. Parse failures, skipped declarations, inactive files,
unmeasured partial declarations and empty reports block verification. The synthetic
compilation only loads platform references: unresolved DI, ASP.NET Core and xUnit
symbols limit external coupling measurements. The real build, project-reference
review and runtime tests are required separately.

Cognitive complexity, fan-in/out, dependency cycles and applicable cohesion are
mandatory review signals, not additional automated numerical gates. Review multiple
cohesion groups for SRP; unintended cycles and reverse core-to-integration dependencies
block acceptance. Mutation testing is not collected and has no passing score.
Review SOLID, British English, public contracts, compatibility and licences explicitly.

## Audit and packaging

`check-audit.ps1` uses version-1 JSON dependency and vulnerability reports, checks
all solution projects and their `net10.0` frameworks, and reconciles the complete
inventory against lock files. nuget.org is the explicit package and audit source.
Every reported vulnerability fails, regardless of severity or process exit status.
Missing/malformed reports, source failures and unexpected diagnostics fail closed.
The tool's bundled dependencies are reviewed separately; this application audit is
not a complete SDK/tool-binary security assessment.

`check-packages.ps1` packs both libraries with MIT notices and metadata, then runs
standalone and integrated consumers outside the repository using package references
only. The standalone runtime configuration must not require ASP.NET Core. This
checks local package consumption; publication requires separate authorisation.

## Skill sources

Portable instructions were read from `dotnet/skills` at
`a6bf43ab61967c30919334986515163ab24d7635`: `csharp-expert`,
`directory-build-organization`, `scaffold-dotnet-test-project` and
`platform-detection`, at the paths recorded in `research.md`.
The Codemetr skills revision recorded during planning is unavailable from its
remote. Implementation instead read the clean local `codemetr/skills` checkout at
`b3d1f948e536fe18903ed9e4fcad5962ec0e0a88`, paths
`plugins/codemetr-csharp/skills/{guard,analyze}/SKILL.md` and
`plugins/codemetr-csharp/references/tool.md`. These sources guide local development;
no host plugin installation or unavailable specialist activation is claimed.

## Coverage collection on Windows and Linux

The test-only package `coverlet.collector` 10.1.0 uses VSTest's in-process completion
handshake. The initial MSBuild driver could lose coverage hits when VSTest terminated
the Windows test host during shutdown. The collector retains the same package version,
MIT licence, complete production-assembly scope and per-library coverage thresholds.
`coverage.runsettings` includes both production assemblies with auto-properties enabled
and no hand-written source exclusions. The verifier requires exactly one GUID-scoped
collector report, ignores TRX attachment mirrors, and preserves the report contents
before applying the existing freshness and coverage checks.

Package-consumer restores use an explicit temporary `NuGet.Config` with inherited
sources cleared and exactly the local package feed plus nuget.org. The explicit
nuget.org audit source is retained. This avoids Windows interpreting repeated CLI
`--source` values as filesystem paths; XML construction preserves feed paths with
spaces and metacharacters. The configuration has dependency-free regression checks.

## Release versions

The normal verifier resolves the matching explicit versions from both library
projects. A release supplies `-PackageVersion`, which consistently overrides restore,
build, pack, metadata checks and isolated consumer references. Invalid version input
or disagreeing default project versions fails before building. Release artefacts
record the package version as well as source SHA and working-tree state.

The manual release workflow reuses the Windows/Linux verifier and publishes the
Linux-produced packages only after all platform checks pass. Its artefact selector
requires the exact source revision and version, a clean successful verification run
and exactly the core/integration pair. See [manual releases](../.github/RELEASING.md).

## Coverage tool and compatibility gates

The existing tool manifest also pins `Codemetr.CSharp.Cover.Tool` **0.1.1** from
nuget.org. The installed marketplace plugin `codemetr-csharp-cover` **0.1.0** is
separate guidance, with `coverage` and `coverage-guard` skills. Plugin installation
does not install or enforce the executable. Source provenance is recorded in
[the feature review](../specs/002-interface-sync-cqrs/coverage-source-review.md).

The complete verifier reuses its fresh Cobertura and TRX files for a mandatory
`codemetr-csharp-cover check`, retains `cover.json` and rejects partial evidence,
missing production sources, unreported members, mismatched revision, tool errors
and failed or skipped tests. The original independent 90% line floor in **each**
library remains mandatory. `codemetr-cover.json` sets aggregate and changed-line
floors to 90% and the documented default CRAP maximum to 30. Branch, per-type and
per-member measurements are reported; no separate numerical branch floor is imposed.
No baseline accepts existing gaps and no handwritten library source is excluded.
Non-packable examples are also instrumented and exercised from tests, including
actual loopback HTTP requests. No handwritten library or example source is excluded;
static metrics continue to cover libraries, tests and examples.

Local verification defaults to `origin/main` as the comparison base; pass
`-CoverageBase <revision>` to select an explicit base. Hosted verification fetches
full history and uses the PR target SHA or the current commit's first parent for
push, manual and release runs. An unresolved base fails instead of reducing scope.
Reports retain the resolved merge base and checked head. A no-change diff does not
waive the complete measurement or per-library checks.

`check-compatibility.ps1` restores published `0.1.0-alpha.1` from nuget.org into an
isolated temporary cache. It compiles and runs an old consumer, replaces only the
library assemblies with the candidate and runs that binary without recompilation.
It then recompiles the unchanged source against the candidate assemblies. This
covers standalone and scoped async dispatch and third-party interface implementers.
It checks exercised compatibility; it does not claim exhaustive equivalence for
all possible external consumers. No public method was removed or given new required
interface members. The additive API has not been published by this feature.
