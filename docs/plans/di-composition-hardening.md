# DI composition corrections and usage guidance

**Status**: ready for specification and design; implementation not started
**Owner**: SharpQrs repository maintainer, Alexander Bleissem
**Origin**: [opposing architecture review](../reviews/2026-10-29-cqrs-di-review.md)
**Reviewed baseline**: `9216f275256afcd70c6dec44cf9651d7cf12f837`

## Bounded outcome

Correct unintended validation across execution modes and align handler selection
validation with the composition from which dispatch actually resolves. Preserve
existing public async APIs, exact routing, setup-only configuration, lazy activation,
atomic failure, shared concrete-handler ownership and all existing quality gates.
Resolve the concrete registration guarantee and document safe practical DI use.
No scanning, pipelines, automatic scopes, retries, performance rewrite or release
is included merely because this review identified improvements.

## Findings and execution order

| Finding | Accountable owner | Next work | Completion evidence |
| --- | --- | --- | --- |
| DI-001 | Repository maintainer | Separate reusable concrete type/ownership validation from mode-specific infrastructure checks | Opposite-mode external dispatcher no longer blocks either synchronous concrete helper; genuine same-mode conflicts still fail atomically |
| DI-002 | Repository maintainer | Reproduce copied-composition cases for all four contract selectors; choose provider-specific validation/finalisation strategy before implementation | Zero/one/two matching unkeyed descriptors in the actual composition are rejected/accepted/rejected, independent of the original collection |
| UX-001 | Repository maintainer | Specify the registration-order and final-composition guarantee for concrete services; reconcile documentation and checks | Both before-helper and after-helper cases have explicit tested outcomes; unsupported replacement is not presented as reliable |
| UX-002 | Repository maintainer | Add ownership, decorator, background-worker and testing recipes | Runnable examples prove intended scope sharing and exactly-once ownership; unsafe aliasing is explicitly identified as application configuration |

## Steps

1. Start a new Spec Kit feature from current main, linking these finding IDs. Clarify
   that independent modes remain independent and handler service selection must use
   the effective composition. Do not rewrite completed historical feature evidence.
2. Plan the small DI-001 refactoring and schedule executed failing command/query
   tests before implementation. Verify both concrete and interface paths, genuine
   conflicts and cross-mode reuse of the same concrete handler.
3. Assess DI-002 using a bounded external design probe. Compare an explicit setup
   validation/finalisation step and provider-factory integration. The standard
   service provider does not publicly expose its complete descriptor collection;
   do not introduce reflection or eager handler activation merely to count services.
   Document compatibility and host adoption costs. Never remove the ambiguity guard
   without replacing it. If public integration requirements must change, obtain the
   required explicit contract approval before implementation.
4. Specify zero/one/two descriptors in copied collections, distinct providers and
   independent registration sets for command/query and sync/async selectors.
   Include mutations made before provider construction; mutation after construction
   remains outside the setup-only contract. Test keyed services separately.
5. Resolve UX-001 alongside the finalisation choice: either enforce uniqueness of
   the owned concrete descriptor at the supported final boundary or narrow the
   documented guarantee and reject unsupported replacement as a recipe.
6. Add a hosted worker example with one asynchronous scope per unit of work and a
   decorated interface example. Exercise resource disposal and shared scoped state;
   demonstrate safe multi-contract concrete selection rather than disposable aliases.
7. Complete Spec Kit Tasks and Analyze, then implement with Red/Green evidence.
   Run the complete existing verifier and compatibility checks, retain measured
   coverage/metrics evidence, and use the normal checked PR path.

## Readiness and dependencies

DI-001 has a reproduced trigger and an identified internal separation. DI-002 has
reproducers and acceptance criteria; its remaining technical decision is the
provider-specific validation boundary. The next step is the bounded design probe
in step 3, owned by the repository maintainer. UX-001's final guarantee depends on
that choice. UX-002 can be specified independently, but its final recipes must
reflect the chosen composition boundary. This is a concrete design work item,
not approval to impose a new public provider factory or break existing consumers.

## Acceptance criteria

- Both synchronous categories coexist with unrelated external async dispatchers,
  and equivalent registration styles select the same valid composition.
- Handler count and lookup concern the actual provider composition. The two copied
  collection counterexamples no longer silently select ambiguity or reject an
  otherwise valid selected service. Regression coverage spans all four selectors.
- Validation activates no handler, missing dependency or resource solely to inspect
  selection. No global mutable state or stale reference to an unrelated collection.
- Missing, ambiguous, duplicate and genuine infrastructure conflicts preserve setup
  state. Keyed services do not count as the selected unkeyed handler.
- Existing registrations and consumer source/binaries continue to work. Transient
  services retain per-resolution semantics; scoped services share only within their
  scope; standalone ownership remains with the caller.
- Concrete ownership, multi-contract/cross-mode sharing and synchronous/asynchronous
  disposal remain correct. Ordinary decorators and singleton workers have tested
  usage guidance with explicit scope boundaries.
- All current build, test, per-library coverage, complete coverage, static metrics,
  audit, sample and package-consumer checks pass locally and on Windows/Linux.
- Every finding above receives a tested fix or a documented accountable scope decision.
  No finding is dismissed solely because the old line coverage was 100%.
