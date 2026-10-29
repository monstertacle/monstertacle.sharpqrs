# Implementation evidence

## Authority and workflow

The user authorised autonomous delivery through the remaining Spec Kit phases.
Clarify assessed the documented assumptions without blocking questions. Plan selected
additive contracts and a pinned tool. Tasks mapped all 14 functional requirements
and seven success criteria. Analyze reviewed 17 tasks against Constitution 2.0.0:
no unmapped requirement, unresolved placeholder, critical conflict or blocking
ambiguity. No extension hooks exist. The requirements checklist has 16 checked
items and no unchecked items; implementation does not rewrite its markers.

## TDD evidence

Executed with SDK 10.0.403, xUnit 2/VSTest, Release configuration through the capped
runner. Compilable signatures were established before missing behaviour was tested.
Raw logs are ignored local artefacts, not substitutes for the assertions in source.

| Slice | Executed Red | Green |
| --- | --- | --- |
| Async contract registration | ContractMappingTests: 2 failed; missing overload behaviour and registration-interface assignability | 2 passed after implementing contract selection and setup interfaces |
| Invalid interface selection | ContractMappingTests plus ContractValidationTests: 9 failed against overload stubs | 9 passed after exact descriptor validation and atomic mapping setup |
| Synchronous core | SynchronousDispatchTests plus SynchronousContractTests: 14 failed against compilable throwing stubs | 14 passed after direct synchronous registry implementation |
| Synchronous DI | SynchronousScopeTests plus SynchronousRegistrationTests: 8 failed against throwing helpers | Passed in the complete suite after scope adapters and validation |
| Coverage evidence checker | CheckCover.Tests.ps1 rejected the accepting stub: expected invalid evidence to be rejected | Negative cases and valid complete evidence pass with real validation |

Later decorator, lifetime, concurrency, keyed-service, example and compatibility checks
are integration/regression evidence; they are not retrospectively labelled Red.

## Complete local verification

Command: `bash ~/dev/scripts/run-capped.sh -- pwsh -NoProfile -File scripts/verify.ps1`.
Run directory: `artifacts/verify-20261029T194057Z-38276df8f3b54bb0b34b430bd5004060`.
This checked the implementation working tree based on `7a964c62a7a9f365566304b80cc5a4d218a90322`;
the manifest records dirty paths. It is not represented as clean committed evidence.
Hosted checks on the committed candidate remain a separate gate.

- All eleven stages passed; Release build has no compiler/analyser warnings.
- **139 tests passed**, zero failures and zero skips, recorded in TRX.
- Core: **100% line coverage**, **18/18** branch outcomes.
- Integration: **100% line coverage**, **67/68** branch outcomes.
- Coverage tool 0.1.1: complete; **53 files/types**, **98.3% lines**, **92.1% branches**,
  **97.4% changed-line coverage**, zero findings, unreported, unmapped or exclusions.
  Tool and collector use different source/line attribution; their totals are not
  interchangeable. Both independent library floors passed.
- Static Codemetr 0.1.0 passed all **110** handwritten C# files/types without
  changing its approved limits. Diff guard is also required before the commit.
- Both examples ran; tests also execute their code in-process, including real
  loopback HTTP command/query requests with distinct scope identities.
- Published 0.1.0-alpha.1 consumer passed before and after candidate DLL replacement,
  without recompilation, including third-party dispatcher implementations. The same
  consumer source then rebuilt and passed with candidate DLLs. This is exercised
  compatibility evidence; existing public asynchronous signatures remain unchanged.
- Both local packages passed metadata checks and isolated standalone/integrated
  consumers. The SDK-pinned clean NuGet consumer cache is removed after each check.
- The complete five-project locked dependency audit passed against nuget.org.

## Design and review

SRP/ISP: setup and runtime dispatch are separate contracts; commands and queries and
sync/async execution are independently consumable. OCP/DIP: decorators and existing
interface services participate through explicit mappings; concrete implementation
selection remains an optional composition-root convenience. LSP: original async
members are untouched; new sync dispatch preserves the same request, result, token,
exception and ownership expectations without task adaptation. Container resolution
is confined to non-owning integration adapters. No core-to-integration dependency.

Ownership checks include caller-owned standalone handlers, scoped and transient
interface services, multi-contract and cross-mode concrete reuse, decorators and
async-only disposal. Registration stays setup-only. Descriptor selection is checked
without activation, including keyed-service exclusion and later ambiguity.

Advisory review: no selected type or namespace dependency cycles. Largest selected
fan-out is 17 in the synchronous composition-root helper, justified by its explicit
registration surface; runtime dispatch remains narrow. Maximum reported cognitive
complexity is 6 in infrastructure validation. Cohesion reports two groups for each
core dispatcher/registry and five test fixtures: source review confirms one registry
or probe responsibility with complementary registration/invocation or disposal
methods. Generic/delegate access is incompletely attributed by this heuristic;
no independent domain responsibility was found. Synthetic compilation has 753
unresolved external-symbol diagnostics limited to the four existing reviewed IDs;
real builds and runtime tests supply separate compilation evidence.

British English, XML contracts, API compatibility, MIT package notices and independent
project boundaries reviewed. New coverage tool metadata declares MIT. Its bundled
runtime lists Roslyn 5.3.0, FileSystemGlobbing 10.0.0 and Codemetr libraries; the
application package audit is not claimed as a complete SDK/tool binary audit.
Mutation testing was not collected and has no passing score.

## Findings and dispositions

| Finding | Disposition and check |
| --- | --- |
| MixedModeTests coupling 25 > approved 20 | Split lifetime and wrong-mode routing scenarios into coherent test classes; complete static guard passed |
| Changed examples rejected as unmeasured | Removed proposed example exclusions, instrumented both examples and referenced them from tests; complete cover check passed with no excluded or unreported members |
| Example failure branches and web executable entry are not fully covered | Actual loopback setup and both HTTP routes are tested; the separate executable smoke check exercises the entry. Uncovered sample assertion-failure branches and entry lines are explicitly reported; agreed library and changed-line floors pass. No additional coverage target was requested |
| One integration branch remains uncovered | Existing concrete-handler duplicate-service branch is not required for passing line gates; reviewed alongside cross-contract and cross-mode reuse tests. Branch coverage has no approved numerical floor |

No unresolved actionable acceptance blocker remains locally. CI on the committed
revision, PR completion and final source provenance remain T017 work; package
publication is outside this feature.
