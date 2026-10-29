# Feature Specification: Interface Composition and Synchronous CQRS

**Feature Branch**: `002-interface-sync-cqrs`

**Created**: 2026-10-29

**Status**: Draft; awaiting approval before Clarify

**Constitution reviewed**: 2.0.0; no amendment proposed

**Input**: Use Spec Kit to improve SOLID design through appropriate interfaces,
explain concrete handler registration, and support synchronous and asynchronous CQRS.

## User Scenarios & Testing

### User Story 1 - Compose handlers through contracts (Priority: P1)

As an application developer, I can select existing handler services through their
contracts. Runtime consumers depend only on dispatch contracts; setup code can use
a narrow registration contract without depending on a concrete dispatcher.

**Why this priority**: Separates implementation selection from application use and
allows decorated or substituted handler services to participate.

**Independent Test**: Select a handler service through its contract and dispatch in
a real validated scope, then exercise standalone setup through an abstraction.

**Acceptance Scenarios**:

1. **Given** an existing scoped handler service, **when** its CQRS mapping selects
   its contract without naming the implementation, **then** dispatch resolves that
   service lazily in the current scope and invokes it once.
2. **Given** a decorator registered as that handler service, **when** dispatch occurs,
   **then** the decorator participates and container-owned resources are disposed once.
3. **Given** a standalone handler supplied through its contract, **when** setup occurs
   through a registration abstraction, **then** independent dispatch consumers can
   invoke it and the handler remains caller-owned.
4. **Given** existing concrete-handler registrations, **when** the upgraded library
   is used, **then** routing, lazy activation and ownership are preserved.

### User Story 2 - Execute synchronous work (Priority: P1)

As an application developer doing synchronous work, I can handle commands and
queries synchronously and obtain direct results without artificial task creation.

**Why this priority**: Provides an honest execution contract for synchronous work.

**Independent Test**: Dispatch a synchronous command and query using both standalone
and scoped composition, asserting results, original errors and disposal.

**Acceptance Scenarios**:

1. **Given** a synchronous handler, **when** matching synchronous dispatch occurs,
   **then** its non-null result is returned directly after one invocation.
2. **Given** a throwing handler, **when** synchronous dispatch occurs, **then** the
   original exception reaches the caller without wrapping.
3. **Given** scoped synchronous handlers, **when** two scopes dispatch requests,
   **then** instances and disposal respect the configured container lifetimes.

### User Story 3 - Select execution modes explicitly (Priority: P2)

As an application developer, I can use both modes in one application and choose
which execution contract each consumer needs.

**Why this priority**: Keeps mixed applications predictable while preserving current
asynchronous consumers and preventing hidden execution conversions.

**Independent Test**: Run both modes together, including one request mapped once
per mode, and verify independent routing, cancellation and lifetimes.

**Acceptance Scenarios**:

1. **Given** existing asynchronous handlers, **when** dispatch is awaited, **then**
   results, exceptions and cancellation retain their published behaviour.
2. **Given** only an asynchronous mapping, **when** synchronous dispatch is attempted,
   **then** a clear missing synchronous mapping error occurs without invoking that
   handler or blocking on its task.
3. **Given** a mapping for each mode, **when** each is dispatched, **then** only its
   selected handler executes.
4. **Given** only a synchronous mapping, **when** asynchronous dispatch is attempted,
   **then** it fails clearly without automatic adaptation or background scheduling.

### Edge Cases

- Null requests, handler instances and actual results are rejected in both modes;
  asynchronous handlers must also return non-null tasks.
- Routing remains exact by concrete request type and result type. Handler service
  contracts do not introduce request inheritance fallback or automatic scanning.
- Duplicate mappings within one mode fail atomically, preserving the original;
  one mapping in each distinct mode is permitted.
- Missing selected handler services fail clearly when resolved; ambiguous multiple
  registrations must fail rather than silently select a handler. Plan must define
  validation timing without activating handlers during setup.
- Rejected registration preserves service descriptors and existing mappings.
- Tokens, including already-cancelled tokens, reach handlers unchanged. Handlers
  control cooperative cancellation in both modes.
- Concurrent dispatch does not imply that application handlers are thread-safe.
- Sharing a container-owned service across mappings must not transfer ownership or
  cause multiple disposal through added aliases.

## Requirements

### Functional Requirements

- **FR-001**: Both commands and queries MUST support scoped mapping through registered
  handler contracts without naming implementation types. Acceptance: Story 1 scenarios
  1 and 2, plus missing and ambiguous service checks.
- **FR-002**: Standalone setup MUST be available through a narrow abstraction separate
  from runtime dispatch; consumers MUST NOT require registration access. Acceptance:
  Story 1 scenario 3 and consumers compiled against dispatch contracts alone.
- **FR-003**: Both commands and queries MUST support genuine synchronous handling and
  dispatch in standalone and scoped compositions. Acceptance: Story 2.
- **FR-004**: Execution modes MUST be independently selectable. The library MUST NOT
  block on tasks, automatically schedule background work or implicitly adapt modes.
  Acceptance: Story 3 scenarios 2–4.
- **FR-005**: Existing asynchronous APIs and concrete-handler registration paths MUST
  retain source, binary and behavioural compatibility with `0.1.0-alpha.1`. Existing
  third-party interface implementers MUST NOT acquire new mandatory members.
  Acceptance: unchanged existing tests and consumers, plus explicit compatibility checks.
- **FR-006**: Both modes MUST preserve exact routing, one handler invocation per call,
  non-null result enforcement and original exception propagation. Tokens MUST reach
  handlers unchanged. Acceptance: commands and queries tested in both compositions
  for nulls, incorrect results, exceptions and cooperative cancellation.
- **FR-007**: Registration MUST finish before dispatch, reject duplicates atomically
  within a mode and permit independent mappings across modes. Acceptance: mixed-mode
  routing and unchanged-state assertions after rejection.
- **FR-008**: Integration MUST preserve lazy scope-local resolution and container
  ownership; standalone handlers MUST remain caller-owned. Acceptance: real scopes,
  decorators and disposal checks under Stories 1 and 2.
- **FR-009**: Documentation and runnable examples MUST explain concrete-type selection,
  contract-based composition, synchronous and asynchronous usage, cancellation,
  lifetimes and concurrency limits. Acceptance: examples execute in verification
  and documentation corresponds to tested contracts.
- **FR-010**: Every added public abstraction MUST identify its consumer responsibility.
  SOLID review MUST assess substitutability and dependency direction rather than count
  interfaces. Acceptance: requirements-to-contract review and no reverse dependency
  from core to integration.

### Key Entities

- **Request and result**: An exact command or query and its non-null result contract.
- **Execution mode**: Synchronous or asynchronous handling, selected explicitly.
- **Handler mapping**: One request/result selection within one execution mode.
- **Composition boundary**: Application setup selecting implementation services while
  runtime consumers depend on dispatch contracts.

## Success Criteria

- **SC-001**: Acceptance checks demonstrate all eight combinations of command/query,
  synchronous/asynchronous execution and standalone/scoped composition.
- **SC-002**: Scoped command and query examples select existing handler services through
  contracts without naming implementation types in their mappings.
- **SC-003**: Existing consumer examples and prerelease compatibility checks pass
  without consumer source changes.
- **SC-004**: Missing, ambiguous, duplicate and wrong-mode mappings fail explicitly;
  rejected setup and wrong-mode dispatch never invoke a selected handler.
- **SC-005**: Each documented ownership and cancellation scenario has a passing
  observable check; each added abstraction has a traceable consumer requirement.

## Assumptions

- Constitution 2.0.0 and the approved metrics profile remain unchanged. Staged approval,
  TDD, per-library coverage and full verification still apply.
- Existing handler and dispatcher interface names retain their asynchronous meaning.
  Additive contract names and signatures are decisions for Plan.
- Synchronous handlers can accept cancellation for cooperative synchronous work;
  cancellation does not imply asynchronous execution.
- Explicit mapping remains the default. Scanning, request polymorphism, distributed
  transport, retries and transactions are outside this feature.
- If additive compatibility cannot be preserved, the design requires a separately
  approved scope change and migration guidance.
- No new dependency, tool version, numerical gate or publication is requested.
- No blocking requirements question was identified during Specify. Clarify must
  still assess these assumptions after the user approves the transition.
