# Implementation evidence: .NET 11 support

**Recorded**: 2026-11-14T08:39:48Z (measured with `date -u`)
**Owner**: implementation session for Alexander Bleissem
**Status**: local acceptance passed; hosted CI is reported separately.

## Authority and workflow

The user approved Constitution 2.1.0 and autonomous phase progression on 2026-11-14.
The installed specification, clarification, planning, tasks, analysis and implementation
skills were read. Disabled extension hooks were inspected; none executed. Active
spec/plan/tasks templates were resolved with the repository's PowerShell entry points.
Prerequisite checks resolved specs/004-dotnet11-support successfully.

Before implementation, read-only analysis found no critical/high issues: all seven
requirements mapped to eleven tasks, all three success criteria map to T011, no
unmapped tasks or unresolved clarification markers. Runtime names are the requested
product constraint. No additional user clarification was necessary.

## Red and Green

- Audit: dual-target clean fixtures failed with `Missing or unexpected target
  framework` before adaptation. Clean reports now pass; missing target, duplicate
  target and vulnerabilities in the second target are explicitly rejected.
- Release: missing/partial framework evidence was accepted before adaptation,
  causing the new rejection assertions to fail. Complete target evidence now passes,
  with incomplete evidence rejected alongside existing revision/version/cleanliness checks.
- Package groups/assets: a minimal validation signature initially accepted absent
  assets, causing the new negative assertion to fail. Both groups/assets now pass;
  absent, duplicate or unsupported groups/assets are rejected.

Existing C# source and public contracts are unchanged; no artificial domain tests
were introduced for project-file changes. Initial .NET 10 clean Release build
passed with zero warnings/errors before changing target configuration.

## Complete local verification

Executed the dedicated SDK installation with PATH and DOTNET_ROOT set as documented
in docs/development.md, through run-capped.sh:
`pwsh -NoProfile -File scripts/verify.ps1`.

Successful run: `artifacts/verify-20261114T083855Z-c96db84c934044b881e58dc9706a76f8/`.
Its manifest records base HEAD `b546ec4458a577c0e4cad717ba4f345c7203049c` and the
modified working tree, SDK 11.0.100, PowerShell 7.6.9 and both target frameworks.
This is working-tree implementation evidence, not a clean release qualification.

| Check | Result |
| --- | --- |
| Locked restore and Release build | Both targets; zero warnings/errors |
| Behavioural tests | 187 passed, zero failed/skipped per target |
| Core library coverage | 100% lines and branches on each target |
| Integration library coverage | 100% lines and branches on each target |
| Complete coverage guard | Each target: 70 files, 99% lines, 93.8% branches, no violations/unreported/unmapped source |
| Static metrics | 136 handwritten files; no violations, parse errors or skipped source |
| Published consumer compatibility | Binary replacement and unchanged-source recompilation passed on net10.0 |
| Dependency audit | Both frameworks of all five projects matched lock files; no reported vulnerabilities |
| Examples | Standalone, HTTP and finite scoped worker passed on each target |
| Packages | Both target groups/assets; four external consumers passed |
| Script regression checks | All verifier-invoked suites passed |

The first complete attempt reached packaging and exposed a multi-group XML check
that treated two empty dependency properties as a non-empty array. The final check
counts actual dependency nodes instead. The successful full rerun above includes
that correction; the earlier run remains historical evidence of the failed attempt.

## Review and limits

The core remains dependency-free. .NET 11 supplies DI abstractions in its core
shared framework: inspected installed runtime assembly and packed nuspec confirm
that SDK pruning omits the redundant package dependency for net11.0, while net10.0
retains the 10.0.14 package dependency. Both lock entries and actual consumers were
checked. No framework reference to ASP.NET Core was added to either library.

SOLID boundaries and ownership are unchanged because no handwritten C# source was
changed. Project-reference direction, public contracts and language/licences were
reviewed. The unchanged metrics hotspots/cohesion signals remain review signals,
not new numerical gates. There are no type cycles or reverse core-to-integration
edges. The reported Web/Worker namespace cycle is within the existing example
assembly: its entry point invokes the worker demonstration and worker types use
that example's request/handler types. This cohesive demonstration does not change
library dependencies or introduce a new architectural cycle. Synthetic compilation
limitations remain documented in
scripts/check-metrics.ps1 and docs/development.md. Existing contract and compatibility
checks provide runtime evidence in addition to source inspection. Mutation testing
was not collected. Tests establish the exercised contracts, not exhaustive external
consumer equivalence. Windows execution requires the hosted CI run; no local Windows
result or package publication is claimed.

## Requirement disposition

FR-001/002: dual targets, both test suites and published compatibility passed.
FR-003: independent coverage evidence and gates passed.
FR-004: both inventories/locks and negative audit cases passed.
FR-005: both package groups and all four consumers passed.
FR-006/007: SDKs pinned, both CI jobs configured, release rejection tests passed,
documentation updated. No policy thresholds or exclusions changed.
SC-001–SC-003: local measurements and consumer results above meet acceptance.
No unresolved local acceptance finding remains. Hosted checks are handled by the
feature branch's normal verification workflow, not asserted from configuration.
