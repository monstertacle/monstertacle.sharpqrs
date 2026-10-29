# DI composition implementation evidence

## Independent mode isolation

ModeIsolationTests executed against reviewed behaviour: 2 concrete-selection cases
failed with unrelated asynchronous infrastructure conflicts; 2 interface-selection
cases passed. This is behavioural Red, not a compilation failure. The shared concrete
type/ownership validation was then separated from infrastructure validation.

A focused regression run covering mode isolation, existing conflict tests,
synchronous registration and mixed-mode sharing passed: 21 tests, zero failures. Full verification of this independent slice also
passed before the later worker example changes. Final verification will identify
the complete candidate. Logs remain ignored
under artifacts/di-mode-{red,green}.log.

At this independent-slice checkpoint, the checked-construction contract decision
was pending. The later approval and implementation are recorded below.


## Worker and decorator example

WorkerExampleTests executed against a compilable empty-result stub: both the
scope/disposal result assertion and cancellation assertion failed. The example
uses existing concrete registration, so it is independent of the pending checked
interface-construction decision. A decorated command and query share the same
scoped unit; each hosted worker iteration creates and asynchronously disposes its
own scope. The worker resolves a scoped application operation, whose dispatchers
arrive through constructor injection. Diagnostic counters are read after disposal;
scoped services are never reused for execution after their scope ends.


## Complete independent-slice verification

Run `verify-20261029T205712Z-c2c719a9d3ea417b98beedec65306755` passed all eleven
stages on the working tree based on `e5eabfeb28c469e52f39cab8d059f9c8e5425bd7`.
147 tests passed, none failed/skipped. Both production libraries passed their
independent 90% line floors. Complete coverage tool 0.1.1 measured 64
files/types: 98.8% lines, 91.6% branches,
100% changed-line coverage, no findings/unreported/excluded
members. Static Codemetr 0.1.0 passed 123 handwritten files. The
real build, dependency audit, examples, old consumer binary/source checks and isolated
packages passed. No threshold, baseline, source filter or runtime package changed.

The initial worker full check exposed uncovered error/entry behaviour. A customisable
Generic Host builder and actual CLI completion check now exercise those scenarios.
The worker error test failed against its new compilable overload stub, then passed
when the existing host run lifecycle was shared by both overloads. Original error
identity is asserted. Four worker tests passed in the complete suite.

This is verification of the implemented independent slice, not completion of DI-002.
Its setup contract decision and dependent tasks remain open. The specification,
proposal and implementation are preserved on branch `003-di-composition`.


Final independent-source verification after preserving the existing concrete-keyed
conflict policy: `verify-20261029T205959Z-6678ecff086748dc8f7a62e85d42cbf9`
passed all eleven stages. Shared validation extraction changes only which execution
mode's infrastructure is inspected; the existing concrete service conflict policy
is retained until the approved preparation design addresses the final composition.

Independent delivery review: the core has no changed source. Existing published
interfaces and concrete construction paths remain intact; the old consumer binary
and unchanged source both passed. Mode validation and concrete type validation now
have separate cohesive responsibilities. The worker's scope factory stays at the
host boundary; scoped application work receives dispatcher interfaces through its
constructor. Original errors and caller cancellation are asserted, and the example's
async resource is disposed exactly once per independently observed unit of work.


## Approved checked construction

The owner's subsequent autonomous instruction explicitly approves the proposed
required construction boundary for unreleased interface helpers. Specify, Clarify,
Plan, Tasks and Analyze are complete under Constitution 2.0.0; no decision remains
pending. Published concrete APIs retain ordinary provider construction.

Executed behavioural Red against compilable preparation stubs produced 18 failures;
provider-factory stubs produced 3 failures with 4 existing valid cases passing.
The preparation implementation then passed 25 focused tests. Eight explicit
unprepared/stale-evidence cases failed against a stubbed boundary check and passed
with the required diagnostic. This last focused correction used a stub after an
initial candidate guard existed; it is not claimed as a first implementation Red.
Compilation-only failures are not counted as Red. Ignored logs are
artifacts/di-preparation-{red,green}.log, di-factory-red.log and di-boundary-red.log.

Selections now carry immutable type requirements rather than closures over the
original collection. Preparation validates the actual final descriptors, records
owned descriptor identity, removes stale receipts, and returns an independent
read-only copy. No handler activation or input mutation occurs. The host factory
validates after container callbacks. All four modes have copied zero/one/two
selection and independent-provider regressions, with keyed, lifetime, ownership,
late replacement/removal, failure-atomicity and real Generic Host checks.

## Delivery review

Core source and dependencies are unchanged. Preparation, concrete ownership checks,
and provider construction have separate responsibilities. Dispatcher contracts
remain narrow; application operations receive interfaces by constructor injection.
The factory delegates actual DI construction and preserves scope-validation options.
Static callbacks retain no collection reference; receipts contain immutable data.
No new runtime package, tool, baseline, exclusion or numerical threshold was added.
British English, public XML contracts, ownership guidance and MIT metadata reviewed.

Static advisory review: no type cycles; the Web/Worker namespace cycle is intentional
within one example assembly, because its entry point invokes the worker and both
use the shared example composition. It introduces no library dependency cycle.
Cohesion's separate method groups reflect registration/execution, lifecycle methods,
and factory delegation over the same owned state; splitting those protocols would
obscure their responsibility. CompositionValidation's bounded descriptor traversal
has complexity 6 and coupling 18, below the approved limits. Synthetic metrics
compilation lacks external DI/xUnit references; the real warning-free SDK build
remains mandatory. No diagnostic allowlist or guard was relaxed. Cognitive complexity
and mutation results are not collected by the pinned tool, and are not claimed as
passing. Remaining example startup branches are reviewed as harness-dependent;
HTTP and finite worker execution are exercised by the verifier as well as tests.


## Complete approved-candidate verification

Run `verify-20261029T212612Z-400b618593e241fdabb37f4f81d3f3de` passed all
11 stages against the working tree based on `082c5d5f286c6ca7a2ddf129ee4926c37f9f79cd`.
The sorted path/content SHA-256 over src, tests, examples and scripts, excluding bin
and obj, is `413cb40961da71a1a76190ad95ae9d231cd5ab092463835f5ad57cf0198f05a5`. Subsequent delivery edits concern evidence only.
SDK 10.0.403, PowerShell 7.6.9, static tool 0.1.0 and coverage tool 0.1.1 were used.
187 tests passed with zero failures/skips. Both libraries achieved 100% lines and
branches independently. Complete scope: 70 files/types, 84 members; 99.0% lines
(415/419), 93.8% branches (122/130), 100% changed lines and 97.8% changed branches
against origin/main at e5eabfe. No coverage findings, exclusions or unreported members.
Static measurement passed all 136 handwritten C# files with no violations.
The real Release build had zero warnings/errors. Audit reported no known vulnerable
packages in all five projects. Published old consumer binary and unchanged source,
standalone and HTTP/worker examples, and isolated consumers of both packages passed.
Packages were built locally for verification; nothing was published to NuGet.
