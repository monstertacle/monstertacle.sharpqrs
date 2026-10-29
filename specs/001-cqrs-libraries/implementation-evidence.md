# Implementation evidence

Updated: 2026-10-29T06:04:36Z

## Authority, phase review and scope

The user requested implementation of `001-cqrs-libraries` with Spec Kit, followed
by commit and push, and explicitly approved Codemetr.CSharp.Tool 0.1.0 and every
numerical limit proposed in the plan. Constitution 2.0.0 governs this work.
Implementation and its prerequisite read-only analysis are within that request;
no package publication, merge or unrelated feature is authorised.

The existing specification, plan, contracts, research, data model and 60 tasks were
read. The cross-artefact review found **26 mapped requirements** (15 FR, 6 QC,
5 SC), **60 tasks**, complete task coverage, no unmapped tasks, no blocking
behavioural contradictions, no ambiguous requirements and no duplicate requirements.
The required order was maintained: existing clarified design/tasks, prerequisite
analysis, approved metrics/tool setup, implementation and verification.
Historic statements that no Git repository existed are superseded by the existing
repository and the user's commit/push instruction; they are not current blockers.

All **18** requirements checklist items passed; markers were left unchanged.
Spec Kit's prerequisite script passed with `SPECIFY_FEATURE_DIRECTORY` set to the
existing absolute feature directory. Its ignored per-checkout feature pointer was
restored. No pre/post extension hooks are configured.

## Tools and sources

- .NET SDK **10.0.403**, C# 14, PowerShell **7.6.9** on Linux.
- Test and production package versions match the research pins; lock files cover
  all five projects. Test-only package pruning is disabled to retain the planned
  explicit DI dependency without NU1510. No warning suppression was introduced.
- Published local Codemetr **0.1.0**, installed from nuget.org and restored via the
  committed manifest. Tool package source commit is
  `a44388dc41c79e02586398162848be1aff8f62b1`.
- .NET portable guidance at `dotnet/skills`
  `a6bf43ab61967c30919334986515163ab24d7635`: the four paths in research.md were
  read and applied for contracts/ownership, early shared MSBuild properties,
  one test container with exact solution wiring and VSTest platform identification.
- The planning Codemetr-skills revision
  `2ead33b09d1f5b5be995a9223d0b73f9fc502c4d` returned HTTP 404 and Git
  `not our ref`. Instead, the clean local checkout at
  `b3d1f948e536fe18903ed9e4fcad5962ec0e0a88` supplied
  `plugins/codemetr-csharp/skills/{guard,analyze}/SKILL.md` and
  `plugins/codemetr-csharp/references/tool.md`. This is a disclosed source-provenance
  adjustment, not a tool-version or metrics-policy change. No plugin was installed.

## Red/Green trace

All behavioural runs used `dotnet test Monstertacle.SharpQrs.slnx -c Release
--no-restore`, wrapped with `bash ~/dev/scripts/run-capped.sh --`.
`--logger trx` and `--results-directory artifacts/tdd` recorded the local raw runs.
Compilation failures encountered during scaffolding were corrected and **not**
counted as behavioural Red. Raw artefacts remain ignored; this trace records the
commands' behaviour and executed outcomes.

| Slice / local TRX stem | Executed Red | Green / regression |
| --- | --- | --- |
| `core-happy` | 2 tests failed on compilable unimplemented registration stubs | 2/2 passed after minimal typed storage and invocation |
| `registration` | 4 failed at actual null/duplicate assertions after valid setup | 6/6 passed; originals preserved |
| `core-concurrency-ownership` | Existing behaviour; no fabricated Red | 11/11 passed, including deterministic overlap, category/instance isolation and retained caller ownership |
| `integration-happy` | 2 failed on compilable registration stubs | 13/13 passed with scoped factories and lazy non-owning adapters |
| `integration-conflicts` | 8 failed on duplicate, concrete-service, dispatcher and abstract-handler assertions | 22/22 passed with validation before descriptor mutation; null collections already passed via existing argument validation |
| `integration-scopes` | Existing scope/adapter behaviour | 28/28 passed with real-container reuse, lazy activation, root rejection and exactly-once sync/async disposal |
| HTTP scope tests | Existing integration behaviour; obsolete host APIs were replaced before execution | Both actual requests reuse dependencies internally, differ across scopes and dispose once; included in later passing suites |
| `failure-contract` | 20 failed for null requests/results/tasks and concrete-type lookup diagnostics | 57/57 passed after ordered core validation, including the result-identity slice |
| `result-identity` | 4 failed with an invalid cast instead of the specified validation error | All passed after exact result validation before invocation |
| `cancellation-isolation` | Existing direct-call/await behaviour | 77/77 passed with unchanged tokens, cancellation after start, exception identity and independent overlapping consumers |
| `selection-regressions` | Existing explicit-selection behaviour | 83/83 passed; no interface scanning, unrelated keyed services and alternate duplicate handlers |
| `metadata-regression-before-refactor` | Existing duplicate rejection | 2/2 passed before moving uniqueness validation ahead of adapter installation; both remain in the final suite |
| Different concurrent request/result types | Existing typed registry behaviour | Both categories pass with overlapping int/string mappings in the same dispatcher |

Dependency-free PowerShell checker harnesses were also developed from executable
stubs: each initially failed with `Expected invalid evidence to be rejected.`
They then passed after implementing its verdict logic. Coverage checks include
100% and the inclusive 90% boundary, below-threshold despite an acceptable average,
missing modules/report, zero lines, stale and malformed reports. Metrics checks
include exits 1/2/127, empty/omitted/parse/inactive/partial evidence and external-symbol
limitations. Audit checks include clean, vulnerable (even with process exit zero),
incomplete, malformed, wrong project/framework/schema and unavailable-source evidence.
Orchestration tests require all nine stages, audit/packaging inclusion and immediate
failure propagation. Synthetic reports are **checker tests**, never audit/coverage
claims about production code.

## Executed candidate verification

The complete `scripts/verify.ps1` passed on Linux in run `verify-20261028T224823Z-4c92034a76424214a40d212820c6b3b5`.
It identified base commit `61607fa9aaaebbdb78743ddf45f00aca746ed77a` **with the
implementation working tree present**, not as a clean committed implementation.
The [source manifest](review/source-manifest.json) freezes the candidate's
source/build/checker/example inputs. Fresh-checkout and hosted outcomes follow below.

- **87 tests executed and passed**, zero skipped/failed.
- Both production assemblies measured **100% line coverage**. Branch coverage is
  **100% core** and **97.36% integration**; no production source exclusion is used.
- Full Codemetr: **71 files / 71 types**, zero violations, parse errors or skipped
  files. The full source inventory matches; no inactive files or unmeasured partial
  declarations. No baseline, per-folder exception or relaxed threshold exists.
- Locked restore and Release build passed with **zero warnings/errors**.
- All five projects/frameworks were reconciled against lock files. nuget.org
  reported **no known vulnerabilities** in the complete solution dependency graph.
- Standalone execution and both live HTTP example routes passed, with distinct
  request-scope identifiers. The script stopped its own ephemeral-loopback host.
- Both prerelease packages packed with MIT notices/readmes and correct metadata.
  Independent temporary package-only consumers ran successfully; the standalone
  runtime has no ASP.NET Core framework requirement. Nothing was published.

## Contract and design review

The public signatures match both contracts, without new public abstraction layers.
Each hand-written C# type has its own file. The core has no package, hosting,
container or SharpClaw source reference. Integration references only core and DI
abstractions. Handler invocation uses bound typed delegates, never reflective
invocation; registration state belongs to each dispatcher or service collection.

Dispatch order is null input, exact request lookup, exact result type, one invocation,
non-null task, asynchronous await and non-null result. Original exceptions and tokens
propagate without interception or wrappers. Generic result arguments remain unconstrained;
null reference/nullable-value results fail, while non-null nullable values and nullable
members pass. `EchoResult(null)` is a valid application-defined non-null absence value.
Open handler types cannot be supplied through closed generic helper arguments;
abstract handlers are explicitly rejected. Helpers validate all conflicts before
mutation, then register each concrete scoped handler once. Adapters return original
tasks, resolve only the current scope and never own disposal. Metadata uniqueness is
checked before installing adapters.

SRP review: dispatcher facades expose one category; the registry owns typed mapping
and execution validation; registration validation owns collection configuration
policy; scoped factories translate immutable metadata, and adapters perform lazy
resolution. These responsibilities do not require additional interfaces. Composition
keeps the core independent; generic handlers remain substitutable subject to documented
non-null contracts. This is the initial prerelease surface, so there is no prior
published binary/source API to preserve and no SharpClaw compatibility promise.

Metric advisory review found **no selected type or namespace cycles**. Maximum
reported member cognitive complexity is **6**, in registration validation. Cohesion
reports separate registration/execution or dispatch/disposal groups in the core
registry/facades and test fixtures. Source review accepts these as one dispatcher
lifecycle or test-resource owner, rather than unrelated reasons to change. Other
cohesion entries marked inapplicable are not passing scores. Fan-in/out show expected
core-contract consumers and integration adapters, without a core-to-integration edge.

The synthetic metrics compilation reports **445** external-symbol diagnostics
(`CS0103`, `CS0234`, `CS0246`, `CS1061`) for xUnit/DI/ASP.NET symbols it does not load.
External coupling is therefore limited; the separately passing real build and project
reference inspection provide compilation and architectural-boundary evidence.
Mutation testing was not commissioned or collected; no mutation score is claimed.

Per workspace instruction, a supplementary TypeSafe semantic review used its official
skill v1.0 and `jev-latest`, returning **jev-1.14.0**. The exact questions, source
digest, raw response and uncertainty are in [the review record](review/srp-review.json).
All four types were classified cohesive (confidence 0.98–1.0). The source review above
adopts the bounded conclusion, rather than treating probabilities as proof or as
approval for Git actions. The reviewer has an explicit insufficient-context option.

Authored prose, identifiers, diagnostics and XML comments were reviewed for British
English. The original MIT notice is retained and packed. No unlicensed reference
source was copied. The metrics tool dependency manifest identifies Roslyn 5.3.0 and
FileSystemGlobbing 10.0.0; their NuGet metadata licence expressions were checked as
MIT and recorded in THIRD-PARTY-NOTICES.md. The solution audit does not claim a complete
SDK/tool-binary vulnerability assessment; this scope is the planned audit boundary,
not an undisclosed passing result.

## Acceptance mapping

| Requirements | Implementation / executed checks |
| --- | --- |
| FR-001, FR-011; QC-001 | Five-project Release build, core project boundary, both isolated package consumers |
| FR-002–FR-004; SC-001, SC-005 | Command/QueryDispatchTests, ConcurrentDispatchTests, MultiContractConcurrencyTests, standalone example |
| FR-005, FR-007; SC-003 | RegistrationTests, InvalidInputTests, ResultContractTests, ResultIdentityTests, RegistrationConflictTests, HandlerSelectionTests, MetadataValidationTests |
| FR-006; SC-003 | CancellationAndFailureTests across both categories and both usage modes |
| FR-008–FR-010; SC-002 | InfrastructureRegistrationTests, ScopedResolutionTests, HttpRequestScopeTests, registration conflict/selection probes |
| FR-012–FR-013 | Both runnable examples, public XML documentation, README, development and quickstart guides |
| FR-014–FR-015; SC-005 | DispatcherOwnershipTests, ScopedResolutionTests, CancellationAndFailureTests; real sync/async disposal |
| QC-002 | Executed Red/Green trace, contract/SOLID source review |
| QC-003 | Separate 100% production line coverage, branch review and checker harness |
| QC-004 | Approved full-scope metrics guard, inventory/partial checks, advisory review |
| QC-005–QC-006 | Language/licence review, pinned skill/tool provenance, shared local/CI entry point |
| SC-004 | Package-only consumers and clean committed fresh-checkout verification passed; see final acceptance below |

Some planned test filenames were consolidated by cohesive behaviour rather than
creating duplicate test copies: ownership includes isolation; cancellation includes
exception/provider isolation; result-contract includes invalid-task probes; scoped
resolution includes lazy activation and sync/async disposal. Generic fixtures serve
both categories and usage modes. The mapping above identifies their actual locations.

## Fresh checkout and hosted CI

The first implementation commit `2d3e40d34ea8933d5a1d940c5fe01da1c9054327` was
pushed. Its full verification passed in a clean fresh checkout in run
`verify-20261028T224933Z-83bf64c5424e4307a810882065de5b1b`; the source manifest
matched every candidate input. The documented web command and both HTTP calls were
also executed successfully from that checkout, then its host was stopped.

Hosted [run 40782893460](https://github.com/monstertacle/monstertacle.sharpqrs/actions/runs/40782893460)
passed Linux. Windows passed the build and all 87 tests but lost MSBuild coverage
hits during test-host shutdown, reporting zero coverage. The gate correctly failed.
This is the remaining finding within T052/T057, not a completed platform check.
The CI repair below remains part of the original feature's acceptance work.


## CI coverage repair

Recorded: 2026-10-29T05:57:55Z

Owner: current implementation session, responding to the user's CI-repair request.
Origin: Windows job `123148756435` in hosted run `40782893460`, with missing-hit-file
warnings despite 87 passing tests. The requested repair resumes the already described
collector proposal. See the research amendment for upstream guidance and licence.

Replaced only the private test coverage driver with `coverlet.collector` 10.1.0.
Production sources and public contracts are unchanged. The regenerated lock file
is audited. VSTest, .NET SDK, Test SDK, xUnit, metric limits and coverage thresholds
retain their existing versions and values. `coverage.runsettings` includes both
complete production assemblies, with auto-property measurement and no source exclusions.

New report-discovery behaviour followed Red/Green: a compilable function stub failed
`tests/Scripts/Verify.Tests.ps1` at the missing-report assertion, then the implementation
passed missing/unique/ambiguous/existing-output cases. A real collector run exposed
the TRX attachment mirror; adding that fixture produced a second behavioural Red
(`Expected exactly one collector report, found 2`). Selecting GUID-scoped collector
output made the regression pass while retaining rejection of multiple original reports.
These deterministic tests plus the real Windows coverage gate decide this repair;
no judgement-only claim or metrics-policy relaxation is involved.

The complete local entry point passed in `verify-20261029T055634Z-33e813665d3d47e8bc2a7df06a04b71d` against
base `2d3e40d` with the repair working tree: 87/87 tests, 100% line coverage for each
production library, 100% core / 97.36% integration branch coverage, all 71 C# files
passing Codemetr, no known solution-package vulnerabilities, both runnable examples
and isolated package consumers passing. The existing coverage-checker cases passed.

A [new repair source manifest](review/ci-collector-source-manifest.json) freezes the
updated inputs; the original reviewed manifest remains intact. Remaining steps:
commit/push the candidate, verify its clean fresh checkout and require both hosted
Windows/Linux jobs to pass before closing T052, T057, T059 and T060.


### Windows package-consumer follow-up

Recorded: 2026-10-29T06:01:17Z

Repair commit `677426a7de32c7fceed4c90ad83d6316d05cee4d` passed a clean fresh checkout
in `verify-20261029T055805Z-2f9a14a17b6f45abb44e438b40e73c59`, with all repair-manifest
inputs matching. Hosted [run 40826389655](https://github.com/monstertacle/monstertacle.sharpqrs/actions/runs/40826389655)
passed Linux. Windows now passed coverage (100% lines for both libraries), metrics,
audit and both example checks. It reached package-consumer restore and failed with
NU1301 because the repeated CLI `--source` HTTPS value became a local Windows path.

Owner and scope remain this feature's implementation/verification work (T054/T057).
The bounded correction writes a temporary `NuGet.Config`, clears inherited sources,
and declares exactly the local package feed and nuget.org, with an explicit audit
source. Consumer restore uses `--configfile`, avoiding multi-source CLI parsing.
No feed, package version, production code or acceptance threshold is added or removed.

`tests/Scripts/CheckPackages.Tests.ps1` first failed against a callable configuration
stub with `The consumer NuGet configuration must be created.` It then passed after
XML-safe config creation, including an absolute feed path with spaces and `&`, exact
HTTPS preservation, two-source scope and explicit audit source. This harness is now
mandatory in the shared verifier. The actual hosted NU1301 failure supplies the
platform regression; the next Windows run must prove its resolution.

Full local verification passed in `verify-20261029T060030Z-a5b1d21b05c2424abd363d5811a82dfd` with the follow-up working tree:
87 tests, both libraries at 100% lines, the metrics/audit gates, web/standalone and both
isolated package consumers all passed. The [follow-up source manifest](review/ci-windows-source-manifest.json)
freezes these inputs separately from the previous collector candidate.


## Final verification and finding disposition

Recorded: 2026-10-29T06:04:36Z

Clean fresh checkout `e2f3b26705b725e0bb50546aa8dd5e908672763c` passed the full entry
point in `verify-20261029T060139Z-7b030d6c690f4e4daf6e507e9f9abe21` with a clean
working tree and every follow-up source-manifest input matching. The previously
executed manual quickstart HTTP commands exercise unchanged example sources;
the new checkout also passed both live routes through the mandatory smoke check.
SC-004 and T059 are fulfilled without sibling source inputs.

Hosted [run 40826717973](https://github.com/monstertacle/monstertacle.sharpqrs/actions/runs/40826717973)
completed successfully for that source revision on **Windows 2025 and Ubuntu 24.04**.
Both downloaded verification artefacts were inspected: each records status `passed`,
87/87 tests, 100% line coverage per library, 100% core / 97.36% integration branch
coverage and all 71 C# files/types passing the unchanged metrics guard. Both jobs
also passed the source/lock-reconciled dependency audit, example smoke checks and
standalone/integrated package-only consumers. The [CI record](review/ci-results.json)
retains source SHA, job links/times, run identities, counts and measured scope.

| Origin finding | Disposition | Change and proof |
| --- | --- | --- |
| Original Windows MSBuild coverage hit loss, run 40782893460 | Resolved | Collector integration in 677426a; complete Windows coverage measured in 40826389655 and 40826717973 |
| TRX attachment mirrors counted as additional collection runs | Resolved | GUID-scoped report selection in 677426a, executable Red/Green report-discovery regression and both platform runs |
| Windows package-consumer NU1301, run 40826389655 | Resolved | XML-safe temporary NuGet configuration in e2f3b26, Red/Green config regression and complete package consumers in 40826717973 |
| Missing committed fresh-checkout / hosted acceptance evidence | Resolved | Clean e2f3b26 checkout and the two successful hosted jobs above |

No implementation or acceptance-blocking finding remains open; no follow-up plan
is needed for these resolved findings. The original planned limits still apply:
synthetic external-symbol metric diagnostics and uncollected mutation testing are
explicitly reported review boundaries, not hidden passing measurements. Production
sources, public contracts and their prior SOLID/language/licence reviews are unchanged.
All 60 implementation/verification tasks are complete and the work is ready for
owner acceptance. There is no package publication or merge as part of this work.
