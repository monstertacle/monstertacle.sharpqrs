# Research: General-purpose CQRS Libraries

**Date**: 2026-10-28 | **Constitution**: 2.0.0
**Scope**: Read-only source and documentation research. No packages installed,
restored, executed or security-cleared; no production code written.

## 1. Reference Behaviour and Source Reuse

**Decision**: Independently implement the small typed command/query/handler/dispatcher
model seen in SharpClaw. Keep exact concrete-type dispatch, duplicate rejection,
token forwarding and original exception propagation. Deliberately change the
reference's nullable-result acceptance to universal null-result rejection.

**Rationale**: This matches the requested starting point while respecting the user's
clarification. No root licence was found in the reference repository, so copying its
source into an MIT project is not assumed to be authorised.

**Alternatives considered**: Directly link or copy reference files (external build or
licensing uncertainty); adopt a larger mediator (unrequested features/dependency).

**Sources**: `../sharpclaw/src/SharpClaw/Cqrs`, especially `ICommand.cs`, `IQuery.cs`,
`CommandDispatcher.cs` and `CachedDelegateDispatcherBase.cs`; general checks in
`../sharpclaw/tests/LocalCqrs`. These paths are provenance, never build inputs.

## 2. Scope-safe Integration and Disposal

**Decision**: Use scoped dispatcher factories and typed non-disposable adapters.
Register each concrete handler once as scoped; adapters resolve that concrete service
from the current scope when invoked. Registration metadata contains types and typed
factory actions, never a captured provider or handler instance.

**Rationale**: Avoids root-provider capture, registration-time provider creation,
eager activation of unrelated handlers and repeated disposal of one instance through
several service aliases. DI access stays inside the integration boundary.

**Alternatives considered**: Singleton dispatchers (scope capture); registering a
single instance under several disposable interfaces (double-disposal risk); a new
handler for every request (unnecessary lifetime options); runtime reflection
(wrapping/complexity and trimming concerns without a requirement).

**Source**: [Microsoft DI guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines).
No AOT or trimming compatibility is claimed without separate verification.

## 3. Dependency Boundary

**Decision**: Production core has no NuGet dependencies. Integration references only
`Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.14. Actual ASP.NET Core
hosting and concrete DI reside in tests/examples.

**Rationale**: The requested integration currently consists of registration and scope
behaviour, not HTTP APIs. It can serve ASP.NET Core without imposing its entire shared
framework on a class library. Naming remains `AspNetCore` to match the requested split.

**Alternatives considered**: `Microsoft.AspNetCore.App` framework reference in the
integration (broader than required); full DI package in production (container building
is not a library responsibility).

**Sources**: [Abstractions 10.0.14 metadata](https://api.nuget.org/v3-flatcontainer/microsoft.extensions.dependencyinjection.abstractions/10.0.14/microsoft.extensions.dependencyinjection.abstractions.nuspec),
[ASP.NET Core library guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/target-aspnetcore?view=aspnetcore-10.0).

## 4. Pinned Toolchain and Test Stack

**Decision**: Use the observed installed SDK 10.0.403, C# 14 and VSTest with one test
project. Do not select Microsoft.Testing.Platform in `global.json`.

| Dependency | Pin | Licence | Purpose |
| --- | --- | --- | --- |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.14 | MIT | Integration registration/resolution APIs |
| Microsoft.Extensions.DependencyInjection | 10.0.14 | MIT | Real container in tests |
| Microsoft.AspNetCore.TestHost | 10.0.14 | MIT | Actual HTTP request-scope tests |
| xunit | 2.9.3 | Apache-2.0 | Behavioural tests |
| Microsoft.NET.Test.Sdk | 18.11.0 | MIT | VSTest discovery/execution |
| xunit.runner.visualstudio | 4.0.1 | Apache-2.0 | Adapter supporting xUnit v2 on .NET 8+ |
| coverlet.msbuild | 10.1.0 | MIT | Per-assembly line/branch measurements |
| Codemetr.CSharp.Tool | 0.1.0 | MIT | Published local metrics guard |

**Rationale**: Published NuGet metadata supports .NET 10 consumption; the adapter major
version need not match the test framework major version. VSTest avoids mixing the
chosen Coverlet MSBuild integration with an incompatible MTP execution model.

**Alternatives considered**: xUnit v3/MTP (valid future direction, unnecessary runner
changes here); separate test projects for both libraries (avoidable duplication);
an unpublished local Codemetr build (not reproducible for independent consumers).

**Evidence**: A read-only research agent checked official NuGet indexes, package
metadata and published Coverlet guidance. The main session additionally verified the
published Codemetr package and TestHost 10.0.14 metadata. Exact package metadata is at
`https://api.nuget.org/v3-flatcontainer/<lowercase-id>/<version>/<lowercase-id>.nuspec`.
See [Coverlet 10.1.0](https://www.nuget.org/packages/coverlet.msbuild/10.1.0) and
[dotnet test modes](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test).

Compatibility here is documentary evidence, not successful runtime verification.
Implementation must verify restore, runner discovery, coverage and the resolved
transitive graph. Licence expressions permit the intended use subject to their
notice requirements; they are not evidence that all bundled components were audited.

## 5. Metrics Profile and Measurement Limits

**Decision**: Propose the stricter profile in [plan.md](plan.md), with opt-in whole
partial-type aggregation, no baseline and full source/test/example measurement.
Classify non-guard measurements explicitly rather than inventing tool capabilities.

**Rationale**: The representative design has small contracts, composed dispatchers,
single-purpose scope adapters and bounded test families. The exact limits need user
approval and subsequent measured validation, not an assertion that unbuilt code passes.

**Alternatives considered**: Defaults (not strict enough); a constitutional fixed
type-line limit (removed at user request); excluding tests (forbidden); a green
`analyze` exit code as acceptance (does not execute the failing guard semantics).

**Sources**: [Published 0.1.0 metadata](https://api.nuget.org/v3-flatcontainer/codemetr.csharp.tool/0.1.0/codemetr.csharp.tool.nuspec)
identifies commit `a44388dc41c79e02586398162848be1aff8f62b1`.
[Defaults at that commit](https://github.com/codemetr/Codemetr.CSharp/blob/a44388dc41c79e02586398162848be1aff8f62b1/README.md)
and [measurement definitions](https://github.com/codemetr/Codemetr.CSharp/blob/a44388dc41c79e02586398162848be1aff8f62b1/docs/measurement-definitions.md)
were checked. Their references to a development version reflect the source at release;
verify the installed 0.1.0 CLI's feature support before relying on aggregation.
The newer skills also document aggregation in 0.1.0. Unsupported required features
block implementation verification; do not silently substitute another version.

## 6. Coverage and Reproducibility

**Decision**: Collect fresh Coverlet reports for both production assemblies from the
single test project. Enforce at least 90% lines separately for each expected assembly;
report and review branches without imposing a new threshold.

**Rationale**: An aggregate percentage or successful run with one missing assembly
could hide a failed constitutional requirement. No production source exclusions.
Use locked restore, central versions, SDK pinning and identical local/CI scripts.

**Alternatives considered**: Solution-wide average (hides weak modules); coverage only
from core unit tests (omits integration); treating coverage as test quality proof
(execution alone does not establish assertion quality).

**Validation to perform later**: Test coverage checker with missing/empty/malformed
reports and one assembly below threshold; reconcile expected module identity; audit
package vulnerabilities including transitives; validate package consumers without
the original source tree. None of these checks has run in this phase.

## 7. Skill Provenance and Application

The following local checkouts were read as source material, not installed as host
plugins. Both skill checkouts had clean working trees when their revisions were read.

| Source | Revision | Read paths and application |
| --- | --- | --- |
| `dotnet/skills` | `a6bf43ab61967c30919334986515163ab24d7635` | `plugins/dotnet/skills/csharp-expert/SKILL.md`: language fallback and ownership contracts |
| `dotnet/skills` | same revision | `plugins/dotnet-msbuild/skills/directory-build-organization/SKILL.md`: early shared defaults, central versions, no premature build edits |
| `dotnet/skills` | same revision | `plugins/dotnet-test/skills/scaffold-dotnet-test-project/SKILL.md`: one bounded test container, exact solution wiring, no scaffolding during planning |
| `dotnet/skills` | same revision | `plugins/dotnet-test/skills/platform-detection/SKILL.md`: agent research on VSTest versus MTP |
| `codemetr/skills` | `2ead33b09d1f5b5be995a9223d0b73f9fc502c4d` | `plugins/codemetr-csharp/skills/guard/SKILL.md`, `skills/analyze/SKILL.md` under the same plugin, and `references/tool.md`: permission-aware setup, real guard exit codes, advisory limits |

The coverage-analysis skill was inspected for applicability but not activated: this
phase has neither reports to interpret nor authorisation to collect coverage.
The ASP.NET Core Web API skill was inspected and is not applicable to the production
library: it owns endpoints, whereas this library owns registration. Its endpoint
guidance is relevant only if developing the later example/test host.

Host-native specialist skills are not available in this session's skill catalogue.
Safe read-only planning used the repository instructions as guidance without claiming
plugin activation or performing host-specific installation commands.

## Research Outcome

All design unknowns are resolved into proposals and documented constraints.
Approval of the plan and metrics profile, permission for later installations, actual
runtime checks and security verification remain explicit gates, not unresolved design
questions or claims of completed implementation.

## Implementation amendment: VSTest coverage collection

Recorded: 2026-10-29T05:57:20Z

The initial `coverlet.msbuild` 10.1.0 choice above is historical. Hosted run
[40782893460](https://github.com/monstertacle/monstertacle.sharpqrs/actions/runs/40782893460)
passed all 87 tests on Windows but reported missing hit files and zero coverage;
the Linux job passed. Coverlet's [documented shutdown issue](https://github.com/coverlet-coverage/coverlet/blob/10a8051216eeaebb661624eef348ee768693fed9/Documentation/KnownIssues.md)
identifies this MSBuild-driver failure mode and recommends collector integration.

The user's subsequent CI-repair request resumes the previously proposed correction.
Replace the private test dependency with `coverlet.collector` **10.1.0**, preserving
VSTest, the existing SDK/Test SDK/xUnit versions and every production requirement.
Published [NuGet metadata](https://api.nuget.org/v3-flatcontainer/coverlet.collector/10.1.0/coverlet.collector.nuspec)
declares MIT and source commit `10a8051216eeaebb661624eef348ee768693fed9`.
Its [VSTest guide](https://github.com/coverlet-coverage/coverlet/blob/10a8051216eeaebb661624eef348ee768693fed9/Documentation/VSTestIntegration.md)
supports the existing VSTest execution model; this is not an MTP migration.

The collector's completion handshake prevents the early-shutdown hit-loss mechanism.
Runsettings select both whole production assemblies. A fresh report must pass the
same independent 90% line gates, with branch reporting unchanged. The complete
solution vulnerability audit is repeated against the regenerated test lock file.
No production package, public contract, metric limit or licence changes.
