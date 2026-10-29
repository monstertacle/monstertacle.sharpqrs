# Implementation Plan: General-purpose CQRS Libraries

**Feature**: `001-cqrs-libraries` | **Date**: 2026-10-28 | **Spec**: [spec.md](spec.md)
**Branch**: `feature/001-cqrs-libraries`; Spec Kit feature identifier `001-cqrs-libraries`.
**Status**: Implementation, metrics profile and local tool installation authorised.
Implementation, local verification, clean committed fresh checkout and hosted
Windows/Linux verification passed. Evidence and resolved CI findings are recorded
in [implementation-evidence.md](implementation-evidence.md).

## Summary

Deliver `Monstertacle.SharpQrs`, a dependency-free in-process CQRS core, and
`Monstertacle.SharpQrs.AspNetCore`, a separate scoped registration integration.
Use typed handler contracts, exact concrete request routing and cached delegates,
without reflective invocation. Reject every null result at runtime. The integration
registers concrete handlers once and installs non-owning scope-aware adapters into
core dispatchers. This reuses core validation without leaking container dependencies
into the core or registering disposable handler aliases multiple times.

The planning command produced design documentation only. No application code, tests, tool
manifests, CI workflows or tasks are created. No installation, publication, Git
initialisation or commit is performed.

## Technical Context

**Language/Version**: C# 14, `net10.0`; pin the observed installed SDK `10.0.403` with
roll-forward disabled and prerelease disabled in the future `global.json`.

**Primary Dependencies**: None in the core. Integration references
`Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.14. ASP.NET Core 10
consumers use their normal shared framework; no HTTP dependency is imposed on the library.

**Storage**: None; per-dispatcher registration maps and per-container metadata only.

**Testing**: One `Monstertacle.SharpQrs.Tests` project referencing both production
libraries: xUnit 2.9.3, Microsoft.NET.Test.Sdk 18.11.0,
xunit.runner.visualstudio 4.0.1, coverlet.collector 10.1.0,
Microsoft.Extensions.DependencyInjection 10.0.14 and Microsoft.AspNetCore.TestHost
10.0.14. Use VSTest, not Microsoft.Testing.Platform. Test dependencies are private
and the project is not packable. See [research.md](research.md).

**Target Platform**: Cross-platform .NET 10; validate Windows and Linux in CI.
PowerShell 7 is the cross-platform verification-script prerequisite.

**Project Type**: Two packable class libraries, one test project, two runnable examples.

**Performance Goals**: No invented latency or throughput promise. Dispatch must be
correct under concurrent use after registration. No per-call reflection or global cache.

**Constraints**: All results non-null; scope-safe resolution and exactly-once disposal;
minimum 90% line coverage for each production assembly; strict approved Codemetr rules;
British English; MIT licensing; no external SharpClaw build input.

**Scale/Scope**: Local single-handler dispatch only. No scanning, pipelines, messaging,
event storage, transactions, live reconfiguration or endpoint-generation feature.

## Constitution Check

Assessed against Constitution 2.0.0 before research and again after design.
“Pass” here means design compliance, not executed build/test/metrics evidence.

| Principle or gate | Before research | After design |
| --- | --- | --- |
| Independent libraries and bounded scope | Pass: two-library scope established | Pass: BCL-only core; narrow integration dependency |
| SOLID and explicit contracts | Pass: required by specification | Pass: composed internal registry and typed adapters; ownership and errors defined |
| TDD and effective tests | Pass: mandatory | Pass: Red/Green order and acceptance matrix specified; no implementation claimed |
| Strict metrics | Pass: thresholds intentionally a planning decision | Pass: complete proposed profile and advisory treatment below; user approval required before implementation |
| Specification first and staged approval | Pass: clarification completed | Pass: stop after Phase 1; tasks require next approval |
| British English | Pass | Pass: authored design documents use British English |
| .NET 10 and traceable skills | Pass | Pass: exact SDK and skill revisions recorded |
| Licensing and dependency checks | Pass: MIT selected | Pass: independent implementation; package licences reviewed; actual restore/audit remains an implementation gate |
| Reproducible checks and CI | Pass: required | Pass: exact proposed entry point, scripts, package pins and two-platform CI described |

No constitutional exception is requested. Tool installation, metrics policy adoption
and progression are not implied by this document's creation. Approving the plan must
explicitly include its proposed metrics profile; subsequent installation still follows
the Constitution's approval requirement.

## Project Structure

### Documentation (this feature)

```text
specs/001-cqrs-libraries/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── core-api.md
│   └── registration-api.md
└── checklists/requirements.md
```

`tasks.md` was generated in the subsequent authorised Tasks phase. It now defines
the implementation order; neither document implies implementation approval.

### Planned Source and Build Layout

```text
Monstertacle.SharpQrs.slnx
global.json
Directory.Build.props
Directory.Packages.props
NuGet.Config
codemetr.json
.editorconfig
.config/dotnet-tools.json
src/
├── Monstertacle.SharpQrs/
└── Monstertacle.SharpQrs.AspNetCore/
tests/Monstertacle.SharpQrs.Tests/
├── Core/
├── Integration/
└── Fixtures/
examples/
├── Standalone/
└── Web/
scripts/
├── verify.ps1
├── check-coverage.ps1
├── check-metrics.ps1
├── check-audit.ps1
└── check-packages.ps1
.github/workflows/verify.yml
```

**Structure Decision**: One test project exercises both production assemblies and
avoids duplicated test infrastructure. Give fixture types unique namespaces; every
declared type gets its own file. Examples are non-packable consumers, not additional
production libraries. No implementation entry point exists yet; paths and commands
in [quickstart.md](quickstart.md) become runnable only after implementation.

Shared nullable, warning, deterministic-build and C# settings belong in root
`Directory.Build.props`, imported early by `Microsoft.Common.props`; project-specific
TFMs, references and packability remain in project files. Root central package
management pins package versions. Do not add nested build hierarchies or custom
targets without need. If late validation is required, `Directory.Build.targets`
is imported late by `Microsoft.Common.targets`; do not rely on project TFMs being
available during early property evaluation.

## Design and Contract Decisions

- Public namespaces match the two library names; see [core contract](contracts/core-api.md)
  and [registration contract](contracts/registration-api.md).
- A private composed dispatch registry owns request metadata and invocation delegates.
  Do not expose an abstract dispatcher base merely to share implementation.
- Registrations use concrete type identity within a command or query dispatcher.
  Successful concurrent dispatch reads a stable registry; callers finish registration
  before dispatch. A concurrent dictionary is acceptable without promising live updates.
- The dispatch sequence is null-input check, exact registration lookup, exact result
  type check, invocation, non-null asynchronous-operation check, await, non-null result
  check, result return. No blanket catch, cancellation precheck or exception wrapping.
- Tests for duplicate rejection, original-registration preservation and invalid
  registration inputs precede the corresponding core validation implementation.
  First make valid registration setup succeed, then establish Red at the actual
  guard assertion before adding that guard; a setup failure is not sufficient.
  Integration conflict, idempotence and registration-atomicity tests precede helper validation. The later
  failure-handling story extends these tests and exercises the complete integrated
  contract; it must not be the first test of behaviour implemented in an earlier story.
- Integration extension methods register metadata and scoped services, never build
  a provider. Scope adapters resolve the selected concrete handler lazily on dispatch.
  They do not implement disposal or own the resolved handler.
- Helper-added duplicate request metadata is rejected during registration before
  mutation. Dispatcher construction also checks metadata before creating adapters.
  Do not silently replace existing handler registrations or duplicate concrete services.
- The supported integration registration surface is the supplied helpers; direct
  `ICommandHandler<,>` registrations are not implicitly discovered. No interface aliases
  are needed for helper-registered concrete handlers. See the integration contract
  for pre-existing concrete registration conflicts.
- Direct `await` with `ConfigureAwait(false)` preserves asynchronous exceptions and
  cancellation. Synchronous handler exceptions also propagate unchanged.
- No generic `notnull` constraint is used to pretend runtime enforcement: nullable
  result arguments remain expressible but all actual null results are rejected.
- Implement independently from the documented reference behaviour. No SharpClaw
  source is copied: no root licence was found there, so source reuse rights cannot
  be assumed from this project's MIT licence.

## Approved Metrics Profile

The user explicitly approved this profile and repository-local installation during
implementation. Use published `Codemetr.CSharp.Tool` 0.1.0.
Enable `aggregatePartialTypes`; retain all default banned type-name suffixes.
No baselines, hand-written-code exclusions or relaxed per-folder overrides.

| Rule | Proposed inclusive limit | Tool default | Design rationale |
| --- | ---: | ---: | --- |
| `maxTypeLines` | 250 | 600 | Bounded contracts and behaviour-focused test classes; not a constitutional fixed size |
| `maxConstructorParameters` | 4 | 6 | Registry/adapters need at most a small set of collaborators |
| `maxPublicMethods` | 8 | 10 | Dispatchers expose two operations; test classes group a bounded behaviour family |
| `maxInstanceFields` | 5 | 8 | Registry and scope adapters require little state |
| `maxCyclomaticComplexity` | 10 | 25 | Validation is a short sequence of guard clauses |
| `minMaintainabilityIndex` | 30 | 15 | Stronger floor without treating the heuristic as a readability proof |
| `maxClassCoupling` | 20 | 40 | Allows BCL and container API types while exposing oversized orchestration |
| `maxDepthOfInheritance` | 1 | 3 | Composition; no dispatcher inheritance hierarchy |
| `maxParameters` | 4 | 7 | Public dispatch/registration operations require few arguments |
| `maxNestingDepth` | 2 | 4 | Flat validation and small test arrangements |

These limits are reasoned design proposals, not fabricated measurements. Contract
types are primarily signatures; dispatchers delegate validation; integration adapters
have one provider dependency; each test class addresses a single behaviour family.
Measure the first TDD slices before expanding implementation. If evidence shows
the profile is inappropriate, return to planning and user approval rather than
loosening limits just to pass. Do not game metrics by fragmenting coherent code.

### Scope and Classification

- Full guard: all C# under `src`, `tests` and `examples`, with the same directories as
  semantic context. Any later hand-written C# directory must be added to both.
  Keep fully qualified type names unique across projects to avoid synthetic-compilation
  symbol collisions; do not copy fixtures between independent compilations.
- Automated gates: every numerical rule above; banned suffixes; aggregated partial
  checks; non-zero tool exit codes; parse errors; missing expected source files or
  empty reports. Future `check-metrics.ps1` preserves the tool exit code and compares
  reported eligible source paths against the discovered hand-written C# inventory.
- Mandatory review: cognitive complexity, fan-in/fan-out, type and namespace graphs,
  unresolved external references, analysis completeness, skipped declarations and
  applicable cohesion. Multiple cohesion groups trigger SRP review; unintended cycles
  block approval. Do not invent numeric failures for advisory measurements.
- Not applicable with evidence: cohesion for types marked inapplicable, and mutation
  scores when no mutation run has been commissioned. Mutation testing is optional
  here; a missing report is not a passing score.
- Codemetr's synthetic compilation does not load the real project/package graph.
  Expected unresolved external symbols must be documented; build results and project
  reference inspection provide the separate compilation/boundary checks. Inactive
  hand-written code or unmeasured partial declarations require resolution, not acceptance
  based solely on exit zero.

## Verification, Coverage and Delivery

- SDK-pinned build uses nullable enabled, C# 14, warnings as errors and SDK .NET
  analysers at `10.0-recommended`. Public production API XML documentation is required.
- Commit central versions and per-project `packages.lock.json`; restore locked in
  verification. Use nuget.org as the explicit package source; no sibling-repository
  feeds or tool builds. CI provisions the exact SDK and PowerShell runtime.
- VSTest runs the one test project with Coverlet collector integration. Filter coverage
  to the two production assemblies, never to selected production types or source files.
  Require each expected assembly to be present with measured executable lines and
  at least 90% line coverage; a total average is insufficient. Report branch counts
  without a new numerical branch threshold. The coverage checker must reject missing,
  stale or malformed reports and be verified using passing and failing report fixtures.
- Use one newly created report directory per verification run to prevent stale evidence.
  No blanket `ExcludeFromCodeCoverage` or hand-written source exclusions.
- Unit tests cover both request categories, exact type/result matching, duplicate
  preservation, null inputs/tasks/results, exception identity, token identity,
  cancellation and independent dispatcher instances. Include completed and delayed
  asynchronous results; use barriers or completion sources rather than sleeps.
- Integration tests use real DI with scope validation. Cover repeated infrastructure
  registration, duplicate selections, missing dependencies, unused handler non-activation,
  same concrete handler fulfilling several contracts, scope identity and exactly-once
  synchronous/asynchronous disposal. A TestHost HTTP probe exercises two actual request
  scopes; endpoints exist only in the test fixture/example, not the library.
- Windows and Linux CI run the same `scripts/verify.ps1` entry point for restore,
  Release build, tests/coverage, full metrics, example smoke checks, package-consumer
  checks and the dependency vulnerability gate. The entry point must invoke
  `scripts/check-packages.ps1` and `scripts/check-audit.ps1`; neither may be skipped
  in CI. Missing component scripts are failures, not optional checks.
- Pack both libraries locally and consume them from a temporary local feed outside
  the source tree to prove package metadata and independence. This is a local consumer
  test, not publication. Require the standalone consumer to run without ASP.NET Core.
- Automated checks do not replace documented review of SOLID, British English,
  advisory metrics, licences and API compatibility. Record all outstanding checks.

### Dependency Audit Verdict

After locked restore, `scripts/check-audit.ps1` runs these .NET 10 commands against
the same candidate solution, with output schema version fixed explicitly:

```powershell
dotnet package list --project Monstertacle.SharpQrs.slnx --include-transitive --no-restore --format json --output-version 1
dotnet package list --project Monstertacle.SharpQrs.slnx --include-transitive --vulnerable --no-restore --format json --output-version 1
```

- Configure nuget.org explicitly as the audit source in `NuGet.Config`. Record
  command, SDK, run identity, audit source, complete dependency inventory and findings.
- Parse the version-1 JSON reports and diagnostic streams, not just process status.
  Every reported vulnerability, at any severity, fails this gate. No automatic
  severity exemption, baseline or suppression is permitted.
- Fail on non-zero process status, unsupported schema, malformed/missing reports,
  reported project/framework errors, unavailable audit sources or diagnostics
  indicating that vulnerability data could not be obtained. Zero exit status alone
  does not mean a clean audit.
- Reconcile the inventory with the solution's projects and target frameworks. A
  project with no package references is explicitly accounted for; absent inventory
  or missing audit coverage must not be inferred to mean no vulnerabilities.
- Create fresh per-run output. Missing, incomplete or mismatched-run evidence fails
  automated acceptance and blocks publication. A clean result means only that the
  configured source reported no known vulnerabilities for the checked dependency graph.
- Check the gate with synthetic clean, vulnerable, incomplete, malformed and
  source-failure reports before implementing its verdict logic. Treat these fixtures
  as checker tests, never as real dependency-audit evidence.
- Review bundled tool dependencies and licences separately; this application package
  audit does not claim complete coverage of the SDK or packaged tool contents.
  Findings require resolution or a separately approved policy amendment, not silent
  exclusion from the report.

Command semantics were checked against
[Microsoft's .NET package-list documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-package-list).
No audit was executed during documentation remediation.

### Fresh-checkout Acceptance

An isolated source snapshot is useful interim evidence only. It does not satisfy
SC-004's fresh-checkout requirement. Final feature acceptance remains blocked until
an authorised Git repository and commit exist and the guide passes from a fresh
checkout. Obtain separate authorisation before Git initialisation or a commit;
neither this plan nor remediation authorises them. Hosted CI evidence must likewise
remain explicitly outstanding until actually executed.

### Acceptance Traceability

| Requirements | Planned evidence |
| --- | --- |
| FR-001, FR-011; QC-001 | Project reference audit, Release build, isolated packaged consumers |
| FR-002–FR-004 | Core command/query contract tests and deterministic concurrency tests |
| FR-005–FR-007 | Negative contract tests in both modes, including every actual null result |
| FR-008–FR-010 | Real-container and HTTP-scope integration tests |
| FR-012 | Executable examples and fresh-checkout quickstart verification |
| FR-013–FR-015 | Contract review, ownership/disposal tests and independent-container tests |
| QC-002–QC-003 | TDD evidence, SOLID review and per-assembly coverage reports |
| QC-004 | Full guard reports, source inventory and advisory-metric review |
| QC-005–QC-006 | Language/licence review, pinned skill references and reproducible check evidence |

## Complexity Tracking

No constitutional violations or exceptions. The integration's scoped adapters are
required by FR-009 and FR-014: binding container-owned handlers directly into a global
dispatcher would capture scopes, while interface aliases could duplicate disposal.
The test server is confined to tests; the core has no reflection-based dispatch,
container, persistence or additional framework dependency.
