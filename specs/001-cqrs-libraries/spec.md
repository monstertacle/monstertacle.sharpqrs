# Feature Specification: General-purpose CQRS Libraries

**Feature Branch**: `feature/001-cqrs-libraries` (created during authorised implementation).
**Feature ID**: `001-cqrs-libraries`
**Created**: 2026-10-28
**Status**: Implemented and locally verified; committed/hosted acceptance gates tracked in implementation-evidence.md.
**Governing Constitution**: 2.0.0, ratified on 2026-10-28.
**Input**: Provide a general-purpose CQRS library based on the local CQRS approach in
SharpClaw, with independent core functionality in one library and ASP.NET Core-specific
registration in another. Follow Spec Kit one phase at a time. Apply the agreed
technology, quality, licensing and British English requirements.

## Clarifications

### Session 2026-10-28

- Q: When a handler returns `null` for a reference-type result, should the dispatcher return it or reject it? → A: Reject every `null` result, including nullable value-type results. Handlers must use an explicit non-null representation for outcomes such as “not found”.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Execute Commands and Queries Without a Web Host (Priority: P1)

As an application developer, I want to register handlers and execute commands and
queries with typed results without adopting a web host or SharpClaw-specific behaviour.

**Why this priority**: An independently usable core is the foundation of the feature.

**Independent Test**: A standalone consumer uses only the core library, registers a
command handler and a query handler, and verifies their results.

**Acceptance Scenarios**:

1. **Given** a registered command handler, **When** its command is dispatched,
   **Then** that handler is invoked exactly once and its result is returned.
2. **Given** a registered query handler, **When** its query is dispatched,
   **Then** that handler is invoked exactly once and its result is returned.
3. **Given** a consumer referencing only the core, **When** it builds and runs,
   **Then** neither the integration library, a web host nor SharpClaw is required.
4. **Given** registrations completed before execution, **When** different requests
   are dispatched concurrently, **Then** each reaches the correct handler and receives
   its own result without cross-request substitution.
5. **Given** a caller-owned handler with disposable resources, **When** the core
   dispatcher is used, **Then** it neither takes ownership nor disposes that handler;
   the caller remains responsible for its lifetime.

---

### User Story 2 - Register CQRS in an ASP.NET Core Application (Priority: P2)

As a web application developer, I want to register CQRS services and explicitly
selected handlers through the application's service registration so that handlers
use dependencies from the current request scope.

**Why this priority**: This supplies the requested integration without coupling the
core to a web application.

**Independent Test**: A consumer application registers both request kinds and executes
them within two separate request scopes, observing dependency identity and disposal.

**Acceptance Scenarios**:

1. **Given** registered CQRS services and selected handlers, **When** a command or
   query is dispatched, **Then** its handler and dependencies are resolved and invoked.
2. **Given** two request scopes and a scoped dependency, **When** handlers execute,
   **Then** they share that dependency within one scope but not between scopes.
3. **Given** scope-owned disposable handlers and dependencies, **When** the scope
   ends, **Then** service management disposes them exactly once, including asynchronous
   disposal where applicable, without additional disposal by dispatchers.
4. **Given** registered CQRS infrastructure, **When** infrastructure registration is
   repeated, **Then** no duplicate infrastructure services are introduced.
5. **Given** several available handlers, **When** only some are selected for
   registration, **Then** unselected handlers remain unavailable for dispatch.
6. **Given** a handler whose dependency cannot be resolved, **When** resolution is
   attempted, **Then** the failure reaches the caller rather than being hidden or
   replaced by a handler from another scope.

---

### User Story 3 - Diagnose Failures and Honour Cancellation (Priority: P2)

As an application developer, I want missing or ambiguous handler registrations to
fail clearly, and cancellation and handler failures to remain observable.

**Why this priority**: Predictable failure behaviour is essential in both usage modes.

**Independent Test**: Behavioural tests exercise registration errors, invalid calls,
cancellation and handler failures through both usage modes where applicable.

**Acceptance Scenarios**:

1. **Given** no handler for a request, **When** it is dispatched, **Then** execution
   fails with a diagnostic identifying the request type and no handler is invoked.
2. **Given** an existing handler registration, **When** another registration for the
   same request type is added, **Then** the core rejects it without replacing the
   original. The integration rejects ambiguity no later than dispatch, before any
   candidate handler executes, even if both registrations name the same handler.
3. **Given** a cancellation token, including one already cancelled, **When** a valid
   registered request is dispatched, **Then** the same token reaches the handler.
   The handler's cooperative cancellation reaches the caller.
4. **Given** a handler failure, **When** dispatch completes unsuccessfully,
   **Then** the original exception reaches the caller without a dispatcher-added wrapper.
5. **Given** invalid request or handler input, **When** it is submitted,
   **Then** it is rejected explicitly rather than producing an unrelated failure.
6. **Given** a rejected duplicate core registration, **When** the original request
   is subsequently dispatched, **Then** its original handler remains usable.
7. **Given** a handler that completes with a null result, **When** its request is
   dispatched in either usage mode, **Then** dispatch fails with a diagnostic
   identifying the request type and the non-null result requirement, regardless
   of whether the declared result type permits null values.

### Edge Cases

- A null request or null handler instance is rejected explicitly.
- A requested result type inconsistent with the registered result type is rejected
  before invoking the handler.
- Every null result is rejected, including null reference-type and nullable
  value-type results. “Not found” and similar valid outcomes require an explicit
  non-null representation supplied by the application; no particular representation
  is imposed by this feature. Non-null results containing nullable members are
  not rejected merely because a member is null.
- A handler returning no asynchronous operation instead of an operation fails clearly.
- One handler may fulfil several distinct request contracts; each concrete request
  type still has exactly one handler registration within its command or query category.
- Dispatch uses the concrete request type. A handler registered for a base request
  does not implicitly handle derived requests.
- An already-cancelled token is forwarded rather than intercepted by the dispatcher;
  handlers remain responsible for cooperative cancellation. Dispatch does not promise
  to interrupt synchronous work already started by a handler.
- Command and query classification expresses intent; the library does not enforce
  query side-effect freedom or provide authorisation or transaction guarantees.
- Registrations are completed before dispatch begins. Concurrent registration and
  execution are outside the supported contract for this version.
- Concurrent dispatch does not imply that caller-provided handlers or their mutable
  dependencies are thread-safe; that responsibility remains with the application.
- Neither library introduces hidden global mutable registrations shared between
  independently configured consumers.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The feature MUST provide two independently consumable libraries: a
  standalone CQRS core and a separate ASP.NET Core registration integration depending
  on the core, never the reverse. Acceptance: Story 1.3 and Story 2.1.
- **FR-002**: The core MUST support distinct commands and queries with typed results,
  asynchronous handlers and separate command and query dispatch capabilities.
  Acceptance: Story 1.1–1.2.
- **FR-003**: Consumers MUST be able to register handler instances explicitly without
  a service container. Acceptance: Story 1 independent test.
- **FR-004**: Dispatch MUST invoke exactly the handler registered for the concrete
  request type and return its result. Concurrent dispatch after registration MUST
  preserve request, handler and result associations. Acceptance: Story 1.1–1.4 and
  the concrete-type edge case.
- **FR-005**: Missing and duplicate registrations MUST fail clearly without silent
  replacement or candidate handler execution. Duplicate core registration MUST leave
  the original usable. Acceptance: Story 3.1–3.2 and 3.6.
- **FR-006**: Dispatch MUST forward cancellation tokens unchanged and propagate
  cooperative cancellation and original handler exceptions without added wrappers.
  Acceptance: Story 3.3–3.4 and the cancellation edge cases.
- **FR-007**: Null requests, null handler instances, incompatible requested result
  types and missing asynchronous operations MUST be rejected explicitly. Every
  null handler result MUST be rejected regardless of the declared result type's
  nullability, with a diagnostic identifying the request type and the non-null
  result requirement. Acceptance: Story 3.5, Story 3.7 and the corresponding edge cases.
- **FR-008**: The integration MUST register dispatch capabilities and explicitly
  selected command and query handlers through application service registration.
  Acceptance: Story 2.1 and 2.5.
- **FR-009**: Integration handlers MUST use dependencies from the current scope;
  service management MUST retain responsibility for the lifetimes and disposal of
  services it owns. Acceptance: Story 2.2–2.3 and 2.6.
- **FR-010**: Repeated infrastructure registration MUST be idempotent without
  permitting duplicate handler registrations. Acceptance: Story 2.4 and Story 3.2.
- **FR-011**: Both libraries MUST build and run without access to the SharpClaw
  source directory. SharpClaw MUST remain unchanged. Acceptance: standalone consumer
  verification with no external source references and review of change scope.
- **FR-012**: Both usage modes MUST have executable behavioural tests and documented
  introductory examples sufficient to reproduce their acceptance scenarios.
  Acceptance: SC-001–SC-004.
- **FR-013**: Public contracts MUST document result and nullability rules, errors,
  cancellation, registration timing, concurrency guarantees and lifetime ownership.
  Acceptance: contract review against the edge cases and Stories 1–3.
- **FR-014**: Core dispatchers MUST leave ownership of supplied handler instances
  with the caller. Acceptance: Story 1.5.
- **FR-015**: Independently configured consumers MUST NOT share hidden global mutable
  handler registrations. Acceptance: configure two consumers with different handlers
  for the same request and verify that each invokes only its own handler.

### Quality and Delivery Constraints

These are existing project constraints, not a selection of implementation mechanisms.

- **QC-001**: The libraries and tests MUST use C# and .NET 10; the integration MUST
  support ASP.NET Core 10. Acceptance: build and consumer checks during implementation.
- **QC-002**: Behaviour MUST be developed through TDD and reviewed for SOLID compliance,
  following Constitution 2.0.0. Acceptance: Red/Green evidence and design review.
- **QC-003**: Each production library MUST achieve at least 90% line coverage, with
  branch coverage additionally reported and reviewed. Acceptance: coverage evidence
  for the same source revision, without exclusions of hand-written production code.
- **QC-004**: All hand-written C# code MUST pass the mandatory Codemetr.CSharp guard
  using an approved profile stricter than its documented defaults. All available
  metrics MUST be classified and reviewed as required by the Constitution. Specific
  thresholds and measurement procedures remain planning decisions.
- **QC-005**: All project-authored content MUST use British English. The project MUST
  retain MIT licensing and required third-party notices. Acceptance: language and
  licensing review, including any copied reference material.
- **QC-006**: Development MUST use applicable skills from `dotnet/skills` and
  `codemetr/skills` with traceable revisions, and reproducible quality checks.
  Acceptance: planning and implementation evidence under the Constitution.

### Key Entities

- **Command**: A typed request expressing an action and producing a result.
- **Query**: A typed request expressing information retrieval and producing a result.
- **Handler**: The implementation responsible for a request-result contract, with
  any application dependencies it requires.
- **Dispatcher**: Routes a request to its unique registered handler.
- **Registration**: Associates a concrete request type with a handler and result
  contract in the appropriate request category.
- **Scope**: The boundary governing the lifetime of scope-owned integration services.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A standalone consumer successfully executes at least one command and
  one query without the original application, the web integration or a web host.
- **SC-002**: A web consumer successfully executes both request kinds in two separate
  request scopes, demonstrating dependency reuse within a scope, separation between
  scopes and exactly-once disposal of owned disposable services.
- **SC-003**: Every defined registration, invalid-input, result-contract, cancellation
  and handler-failure scenario has a passing automated acceptance check in each
  applicable usage mode.
- **SC-004**: Following the introductory instructions from a fresh checkout, a
  developer can build and verify both usage modes without the SharpClaw directory
  or undocumented setup steps.
- **SC-005**: All documented ownership, isolation and concurrency guarantees have
  passing behavioural checks, with no registration leakage between independent consumers.

## Assumptions

- The target users are application developers. The core is an in-process dispatch
  library, not a messaging system or a security boundary.
- The reference behaviour is the local CQRS code under
  `../sharpclaw/src/SharpClaw/Cqrs` and the general dispatcher checks under
  `../sharpclaw/tests/LocalCqrs`. These are analysis references, not build inputs.
- This first version uses requests with results, explicit handler selection and
  asynchronous execution. Resultless request variants and automatic assembly scanning
  are not required. Registration is a setup step, not a live reconfiguration feature.
- Public signatures, package names, framework dependency details, testing tools,
  tool versions and metric thresholds are deferred to technical planning.
- No throughput or latency target has been requested. Correctness under concurrent
  dispatch is required; unsupported performance claims will not be introduced.
- Event sourcing, event stores, distributed messaging, persistence, automatic
  transactions, generated HTTP endpoints, middleware pipelines and migrating
  SharpClaw to these libraries are outside this feature's scope.
- MIT licensing is already selected. Publication is not authorised by this feature.
  CI remains a constitutional requirement before the first merge or publication;
  its concrete arrangement is a planning concern.
- This specification updates the existing `001-cqrs-libraries` feature rather than
  creating a second feature for the same request. `.specify/feature.json` identifies
  the feature independently of any Git branch. No Git initialisation, branch creation
  or commit is authorised by this specification step.
- Constitution 2.0.0 governs acceptance. Completing this specification does not
  authorise clarification, planning or implementation without the user's next approval.
