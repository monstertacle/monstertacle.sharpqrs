# Feature Specification: Correct DI composition and practical usage

**Created**: 2026-10-29
**Status**: Clarified; checked construction explicitly approved
**Authority**: User authorised autonomous Spec Kit implementation of the opposing review follow-up.
**Origins**: DI-001, DI-002, UX-001 and UX-002 in docs/plans/di-composition-hardening.md.

## User Scenarios and Testing

### Story 1 — Independently compose execution modes (P1)

Developers can select sync handlers while another module supplies async dispatchers,
using either concrete or interface selection. Genuine same-mode conflicts remain errors.
Acceptance: command/query × concrete/interface × external opposite-mode dispatcher
combinations succeed, preserve cancellation and do not mutate descriptors on failure.

### Story 2 — Validate the effective composition (P1)

Developers can copy service descriptors, customise the copy before building, and
build independent providers whose mappings validate their own final composition.
Acceptance: each of four contract selectors rejects zero/two matching unkeyed
services, accepts one and stays independent of edits to the original collection.
No validation activates handlers. Prepared snapshots cannot be mutated accidentally.

### Story 3 — Make ownership and usage explicit (P2)

Developers can choose safe handler/decorator composition and run scoped work from a
singleton worker without capturing or prematurely disposing scoped dependencies.
Acceptance: runnable decorated and worker examples use explicit units of work,
share intended scoped state, propagate tokens/errors and dispose resources once.

## Requirements

- **FR-001**: Sync concrete selection MUST validate only its own infrastructure plus
  shared handler type/ownership rules. Acceptance: Story 1 regression matrix.
- **FR-002**: Each mapping's service selection MUST describe the same final composition
  from which its provider resolves. Acceptance: Story 2 copied collection matrix.
- **FR-003**: Validation MUST reject missing, ambiguous and conflicting owned concrete
  selections without activating handlers or mutating the input collection.
- **FR-004**: Final prepared metadata MUST be immutable and provider-specific; no
  captured mutable source collection or global state may influence later resolution.
- **FR-005**: Duplicate mappings and keyed/unkeyed selection semantics MUST retain
  explicit errors and exact routing. Failed preparation MUST leave its input intact.
- **FR-006**: Published 0.1.0-alpha.1 source/binary consumers and concrete helper
  paths MUST remain compatible. No existing dispatcher/handler interface gains members.
- **FR-007**: Transient resolution, scoped sharing, caller/container ownership,
  async disposal, original exceptions and token propagation MUST remain intact.
- **FR-008**: Documentation and runnable examples MUST explain checked composition,
  decorators, safe shared concrete services, tests and singleton worker scopes.
- **FR-009**: All four review findings MUST have explicit tested dispositions under
  unchanged coverage, metrics, audit and compatibility gates on Windows/Linux.

## Success Criteria

- **SC-001**: Both synchronous request categories work beside external async dispatchers
  through both selection styles; genuine conflicts preserve the original setup.
- **SC-002**: Every contract selector passes the zero/one/two descriptor matrix on a
  customised copy and two independent providers, with zero validation activations.
- **SC-003**: Decorated and worker scenarios pass with token/error preservation,
  scope isolation and exactly-once disposal. No implicit per-dispatch scope is added.
- **SC-004**: Complete local/hosted verification and published compatibility pass.

## Clarifications

The standard service-provider API exposes service resolution, not its descriptor
inventory. Automatically detecting duplicate descriptors from an already-built
provider requires eager activation or private implementation inspection. Both are
excluded. The proposed checked setup boundary is described in contracts/composition.md.

The user explicitly approved autonomous implementation after the checked-construction
proposal was presented. The required boundary is approved, including migration of
the unreleased interface helpers; published concrete APIs remain compatible. No
blocking requirements question remains. Autonomous progression covers all phases.

## Assumptions and Limits

No core routing rewrite, scanning, pipeline, automatic scope, tool upgrade or package
publication. Existing static limits, per-library 90% line floor and full coverage
policy remain unchanged. The integration retains its DI-abstractions-only dependency.
