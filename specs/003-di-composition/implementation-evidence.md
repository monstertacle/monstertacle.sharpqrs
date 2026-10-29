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

The required checked-construction contract change is still awaiting the explicit
owner decision. No dependent runtime implementation is claimed.


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
