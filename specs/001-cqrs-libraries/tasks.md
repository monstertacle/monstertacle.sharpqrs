# Tasks: General-purpose CQRS Libraries

**Input**: Design documents in `specs/001-cqrs-libraries/`.
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md),
[data-model.md](data-model.md), [core contract](contracts/core-api.md),
[registration contract](contracts/registration-api.md) and Constitution 2.0.0.
**Status**: Implementation authorised; T001–T058 implemented and locally verified except hosted-CI execution gates. See implementation-evidence.md for actual paths, Red/Green results and remaining committed/hosted checks.
**Tests**: Mandatory. The specification and Constitution require TDD, real-container
integration tests, per-production-library coverage and strict code metrics.
**Organisation**: Tasks are grouped by user story. Infrastructure supports all stories;
each story has its own observable checkpoint. This file does not authorise implementation.

## Format: `[ID] [P?] [Story] Description`

- Every task has an unchecked box, sequential identifier and explicit repository-relative paths.
- `[P]` identifies independent file work within an unlocked dependency stage, not permission
  to ignore prerequisites or to run concurrent builds against the same `bin`/`obj` paths.
- `[US1]`, `[US2]` and `[US3]` map to the corresponding specification stories.
- No story labels are used on shared setup, foundations or final verification tasks.

## Path Conventions and Execution Rules

- Production: `src/Monstertacle.SharpQrs/` and `src/Monstertacle.SharpQrs.AspNetCore/`.
- One test project: `tests/Monstertacle.SharpQrs.Tests/`, with `Core/`, `Integration/`
  and `Fixtures/` subdirectories. Additional gate-script checks use dependency-free
  PowerShell harnesses under `tests/Scripts/`; no Pester dependency is introduced.
- Examples: `examples/Standalone/` and `examples/Web/`, each with a unique namespace
  and explicit `Program` type to avoid conflicting synthetic-compilation identities.
- Every declared C# type, including fixtures and nested-type candidates, gets its own file.
- Before each relevant activity, read applicable skills at the revisions recorded in
  `research.md`. Do not claim host plugin activation or install anything without consent.
- For every new behaviour, introduce a compilable contract/stub if necessary, write
  its test, run it to establish a behavioural Red, implement minimally, then rerun Green
  and refactor. Execute small test/implementation pairs within each task group rather
  than accumulating an entire unimplemented suite. Never count compilation failure as Red.
- A later test may already pass because an earlier slice established the behaviour.
  Record that as an existing-behaviour regression check; do not break code to fabricate
  Red. Any further behaviour-changing fix still requires an appropriate failing test.
- Record commands, tested behaviours and outcomes in
  `specs/001-cqrs-libraries/implementation-evidence.md`. Generated raw reports belong
  in ignored `artifacts/`, not in committed evidence.
- Quoted model constraints below are normative excerpts from `data-model.md`;
  the full public contracts govern all other details.

## Phase 1: Setup — Shared Infrastructure

**Purpose**: Establish the approved toolchain and build layout without implementing behaviour.

- [X] T001 Verify implementation-phase approval and obtain explicit approval of the proposed metrics profile and repository-local tool installation before proceeding; record the scope, Constitution 2.0.0 and pinned skill revisions in `specs/001-cqrs-libraries/implementation-evidence.md`; do not initialise Git, commit or publish without separate authorisation.
- [X] T002 Create `global.json`, `Directory.Build.props` and `.editorconfig` for SDK 10.0.403 with roll-forward/prerelease disabled, C# 14, nullable enabled, deterministic builds, warnings as errors and SDK analysers at `10.0-recommended`; keep TFMs and references in individual project files and require production public API XML documentation.
- [X] T003 Create `Directory.Packages.props` and `NuGet.Config` with nuget.org as the explicit feed and the exact pins in `specs/001-cqrs-libraries/research.md`: DI Abstractions/DI/TestHost 10.0.14, xunit 2.9.3, Test SDK 18.11.0, adapter 4.0.1 and Coverlet collector 10.1.0; enable per-project lock-file generation without introducing preview packages or MTP.
- [X] T004 Create `src/Monstertacle.SharpQrs/Monstertacle.SharpQrs.csproj` and `src/Monstertacle.SharpQrs.AspNetCore/Monstertacle.SharpQrs.AspNetCore.csproj` targeting `net10.0`; keep the core BCL-only and give the integration only the core project reference and DI Abstractions package reference; configure MIT metadata for both packable libraries.
- [X] T005 Create `tests/Monstertacle.SharpQrs.Tests/Monstertacle.SharpQrs.Tests.csproj` targeting `net10.0`, referencing both libraries and the pinned test stack; set test/non-packable properties and private test dependencies, use VSTest and add the ASP.NET Core framework reference needed by TestHost fixtures.
- [X] T006 Create `Monstertacle.SharpQrs.slnx` containing exactly the two initial production projects and the one test project, perform the approved initial restore, generate each project's `packages.lock.json`, verify the exact solution membership and build in Release; do not report zero discovered tests as successful behavioural verification.
- [X] T007 After T001 approval, create `.config/dotnet-tools.json` pinning published `Codemetr.CSharp.Tool` 0.1.0 and `codemetr.json` implementing the exact approved profile in `specs/001-cqrs-libraries/plan.md`, including partial aggregation, all default banned suffixes and `src`/`tests`/`examples` context; restore the local tool, verify CLI capabilities and reject baselines, relaxed overrides or hand-written-code exclusions.

**Checkpoint**: Exact toolchain and dependencies restore/build; metrics profile adoption
and installation consent are evidenced. No library behaviour or passing test suite is claimed.

## Phase 2: Foundational — Blocking Contracts and Fixtures

**Purpose**: Create only the minimum compilable surface needed for behavioural TDD.

- [X] T008 Create documented invariant request interfaces and typed handler/dispatcher interfaces in `src/Monstertacle.SharpQrs/ICommand.cs`, `IQuery.cs`, `ICommandHandler.cs`, `IQueryHandler.cs`, `ICommandDispatcher.cs` and `IQueryDispatcher.cs` following `specs/001-cqrs-libraries/contracts/core-api.md`; keep `TResult` unconstrained so runtime nullable-result probes remain expressible.
- [X] T009 Add minimal documented public constructor/registration/dispatch signatures in `src/Monstertacle.SharpQrs/CommandDispatcher.cs` and `src/Monstertacle.SharpQrs/QueryDispatcher.cs` with intentionally unimplemented bodies, allowing subsequent tests to fail on behaviour rather than absent symbols; do not pre-implement routing or validation.
- [X] T010 Add deterministic request, result and counting-handler fixtures in `tests/Monstertacle.SharpQrs.Tests/Fixtures/EchoCommand.cs`, `EchoQuery.cs`, `EchoCommandHandler.cs`, `EchoQueryHandler.cs` and `EchoResult.cs`; keep each type in its own file, give fixtures no global mutable state and verify the scaffold compiles before starting story tests.

**Checkpoint**: Public contracts compile. The next tests must discover and execute real
production calls; scaffolding is not a substitute for the Red/Green cycle.

## Phase 3: User Story 1 — Execute Commands and Queries Without a Web Host (P1)

**Goal**: Deliver an independently usable core with typed routing and caller-owned handlers.
**Independent Test**: Register one command and one query handler using only the core,
verify exactly-once execution and expected results, and demonstrate concurrent routing,
independent dispatcher instances and retained caller ownership.

### Tests First

- [X] T011 [P] [US1] Write and execute happy-path behavioural Red tests in `tests/Monstertacle.SharpQrs.Tests/Core/CommandDispatchTests.cs`; prepare `CommandRegistrationTests.cs` for null handler rejection, duplicate command/result mapping rejection and original-registration preservation, executing each guard probe in T014 after its valid setup succeeds but before implementing that guard; cover explicit registration, concrete routing, completed/delayed results and exactly-once invocation (FR-002–FR-005, FR-007).
- [X] T012 [P] [US1] Write and execute equivalent happy-path query Red tests in `tests/Monstertacle.SharpQrs.Tests/Core/QueryDispatchTests.cs`; prepare `QueryRegistrationTests.cs` and query fixtures under `Fixtures/` for null handler rejection, duplicate query/result mapping rejection and original preservation, executing guard probes in T015 after valid setup succeeds and before any query-specific guard is added; include completed/delayed results and category independence (FR-002–FR-005, FR-007).

### Minimal Implementation and Further Behavioural Slices

- [X] T013 [US1] Introduce only the composed internal storage and bound invocation delegates needed by the failing happy-path tests in `src/Monstertacle.SharpQrs/DispatchRegistry.cs` and `HandlerRegistration.cs`; use concrete request keys without a public base class or reflective invocation, and defer deliberate duplicate/null rejection logic until T014's public guard probes reach and fail at that behaviour.
- [X] T014 [US1] Wire command registration/dispatch in `src/Monstertacle.SharpQrs/CommandDispatcher.cs` and turn T011 happy paths Green; then run each prepared registration probe with valid setup, establish Red specifically at the missing guard, and add the minimal registry/dispatcher validation to reject null/duplicates while preserving the original mapping; record an initial full-source Codemetr check in `specs/001-cqrs-libraries/implementation-evidence.md` without relaxing thresholds.
- [X] T015 [US1] Wire query registration/dispatch in `src/Monstertacle.SharpQrs/QueryDispatcher.cs`, turn T012 happy paths Green and execute its prepared registration probes; record already-passing shared-registry cases as regressions and establish a behaviour-specific Red before adding any query-specific guard; preserve concrete-type routing and category independence.
- [X] T016 [P] [US1] Add deterministic overlapping-dispatch checks in `tests/Monstertacle.SharpQrs.Tests/Core/ConcurrentDispatchTests.cs` using completion sources or barriers, not sleeps, covering both categories and distinct request/result associations; establish Red before any necessary concurrency refinement (FR-004).
- [X] T017 [P] [US1] Add ownership and independent-instance checks in `tests/Monstertacle.SharpQrs.Tests/Core/DispatcherOwnershipTests.cs` and `DispatcherIsolationTests.cs`, with disposable fixtures in `Fixtures/`, enforcing “It does not transfer ownership or create a lease” and “No task or handler instance is stored in global mutable state” (FR-014–FR-015).
- [X] T018 [US1] Make only test-driven routing/concurrency/ownership refinements in `src/Monstertacle.SharpQrs/DispatchRegistry.cs`, `CommandDispatcher.cs` and `QueryDispatcher.cs`, turn T016–T017 Green or document already-passing regression checks, and rerun the entire US1 test group before refactoring.
- [X] T019 [US1] Create `examples/Standalone/Standalone.csproj` referencing only the core, add it to `Monstertacle.SharpQrs.slnx` and generate `examples/Standalone/packages.lock.json`; verify no integration, ASP.NET Core or SharpClaw source dependency enters this consumer (FR-001, FR-011).
- [X] T020 [US1] Implement the standalone demonstration in `examples/Standalone/Program.cs`, `EchoCommand.cs`, `EchoQuery.cs`, `EchoCommandHandler.cs`, `EchoQueryHandler.cs` and `EchoResult.cs`, with one type per file; execute both requests, assert expected results/counts and leave disposal with the caller (FR-012, SC-001).
- [X] T021 [US1] Complete XML contract documentation in `src/Monstertacle.SharpQrs/` and standalone instructions in `examples/Standalone/README.md`, then record passing US1 tests, sample execution, project-boundary review and full metrics results in `specs/001-cqrs-libraries/implementation-evidence.md`; do not claim final error-contract or two-library acceptance yet (FR-013, SC-004–SC-005).

**Checkpoint / MVP**: US1 can be demonstrated without ASP.NET Core or SharpClaw.
US3's full negative-contract suite and final quality gates remain mandatory before release.

## Phase 4: User Story 2 — Register CQRS in ASP.NET Core (P2)

**Goal**: Add explicit handler selection and scope-safe resolution without transferring
ownership to dispatchers or disposing one concrete handler through several aliases.
**Independent Test**: Build a real container with scope validation, execute both request
kinds in two scopes and actual TestHost requests, and verify identity and exactly-once disposal.

### Tests First

- [X] T022 [US2] Add documented minimal extension-method stubs in `src/Monstertacle.SharpQrs.AspNetCore/SharpQrsServiceCollectionExtensions.cs` with the three exact registration signatures from `specs/001-cqrs-libraries/contracts/registration-api.md`; introduce no implementation until the following registration tests establish behavioural Red.
- [X] T023 [P] [US2] Write Red tests in `tests/Monstertacle.SharpQrs.Tests/Integration/InfrastructureRegistrationTests.cs`, `HandlerSelectionTests.cs`, `RegistrationConflictTests.cs` and `RegistrationAtomicityTests.cs` for idempotence, original-collection return, optional infrastructure setup and selected-only availability; before T027–T028 validation, establish Red for null collections, identical/different duplicate mappings, invalid abstract handlers, external concrete/dispatcher conflicts and descriptor preservation after failure, in both categories (FR-005, FR-007–FR-008, FR-010).
- [X] T024 [P] [US2] Write Red tests in `tests/Monstertacle.SharpQrs.Tests/Integration/ScopedResolutionTests.cs` and `LazyActivationTests.cs` with dedicated fixtures under `Fixtures/Scopes/`, covering same-scope reuse, cross-scope separation and no activation of unrelated handlers; enable scope validation and assert no root-provider fallback (FR-009).
- [X] T025 [P] [US2] Write Red tests in `tests/Monstertacle.SharpQrs.Tests/Integration/HandlerDisposalTests.cs` and `AsyncHandlerDisposalTests.cs` with dedicated fixtures under `Fixtures/Disposal/`, including one concrete handler serving several command/query mappings; enforce “Concrete handler types are registered only once even when selected for multiple distinct contracts” and exactly-once container disposal (FR-009).
- [X] T026 [P] [US2] Write the real HTTP-scope Red probe in `tests/Monstertacle.SharpQrs.Tests/Integration/HttpRequestScopeTests.cs` and host/DTO/handler fixtures under `Fixtures/Http/`, using TestHost and deterministic completion signals to verify reuse within each request, separation between two requests and disposal after completion (SC-002).

### Implementation

- [X] T027 [US2] Add immutable typed registration metadata and helper-owned concrete-service tracking in `src/Monstertacle.SharpQrs.AspNetCore/CommandHandlerRegistration.cs`, `QueryHandlerRegistration.cs` and `RegisteredHandlerType.cs`, preserving “Metadata is immutable after service setup. It captures no provider or handler” and unique category/request identities.
- [X] T028 [US2] Implement helper registration in `src/Monstertacle.SharpQrs.AspNetCore/SharpQrsServiceCollectionExtensions.cs` in T023-driven slices: first make valid setup work, then establish Red at each actual duplicate/conflict/atomicity assertion before adding its rule; turn those checks Green with validation before descriptor mutation and each concrete handler registered once as scoped; never build a provider, resolve handlers during setup or add disposable interface aliases.
- [X] T029 [US2] Implement non-disposable adapters in `src/Monstertacle.SharpQrs.AspNetCore/ScopedCommandHandler.cs` and `ScopedQueryHandler.cs`, resolving the selected concrete handler from the current provider on invocation and enforcing “Adapter calls return the original handler task without an asynchronous wrapper”.
- [X] T030 [US2] Implement scoped dispatcher construction in `src/Monstertacle.SharpQrs.AspNetCore/ScopedDispatcherFactory.cs`, validate metadata before adapter installation, register the factories through `SharpQrsServiceCollectionExtensions.cs` and turn T023–T024 Green without root scope capture or eager handler creation.
- [X] T031 [US2] Turn T025–T026 Green with only test-driven lifetime/refinement changes in `src/Monstertacle.SharpQrs.AspNetCore/`; verify exactly-once synchronous and asynchronous disposal, multi-contract concrete reuse and real request-scope boundaries before refactoring.
- [X] T032 [US2] Add independent-provider and concurrent integrated-dispatch tests in `tests/Monstertacle.SharpQrs.Tests/Integration/ProviderIsolationTests.cs` and `ConcurrentScopedDispatchTests.cs`; use different mappings for the same request in separate providers and thread-safe fixtures, applying only failing-test-driven fixes to `ScopedDispatcherFactory.cs` or adapters (FR-004, FR-015).
- [X] T033 [US2] Create `examples/Web/Web.csproj` and `examples/Web/packages.lock.json`, register it in `Monstertacle.SharpQrs.slnx`, and implement explicit `Program.cs` plus separately filed request/result/handler/scope types exposing only the two demonstration routes documented in `specs/001-cqrs-libraries/quickstart.md`; use loopback binding, scope validation and the library registration helpers, not generated endpoints (FR-012).
- [X] T034 [US2] Document registration selection, scoped lifetimes, unsupported external service conflicts and ownership in `src/Monstertacle.SharpQrs.AspNetCore/SharpQrsServiceCollectionExtensions.cs` XML comments and `examples/Web/README.md`; include sample HTTP calls and safe shutdown instructions without promising handler thread safety (FR-013).
- [X] T035 [US2] Run core and integration regression tests, both example smoke checks and full-source metrics; record DI scope validation, HTTP-scope evidence and disposal counts in `specs/001-cqrs-libraries/implementation-evidence.md`, explicitly distinguishing code coverage still to be gated in Phase 6.

**Checkpoint**: Both usage modes are independently demonstrable; no scope capture,
unselected handler activation or duplicate disposal is accepted. Proceed to the full
failure contract, not to publication.

## Phase 5: User Story 3 — Diagnose Failures and Honour Cancellation (P2)

**Goal**: Make invalid configurations and calls fail predictably while preserving
original exceptions and cooperative cancellation in both modes.
**Independent Test**: Run negative-contract suites through core and integrated dispatch,
including all null-result forms, duplicate preservation, exception identity and token identity.

### Tests First

- [X] T036 [P] [US3] Add Red tests for remaining missing-handler, base-only-handler/derived-request and null-request behaviour in `tests/Monstertacle.SharpQrs.Tests/Core/RegistrationFailureTests.cs` and `InvalidInputTests.cs` before any corresponding T041 implementation; rerun T011–T012's existing null-handler/duplicate/original-preservation tests rather than duplicating them or treating this phase as their first validation; assert exception types and full request identities (FR-005, FR-007).
- [X] T037 [P] [US3] Add result-contract probes in `tests/Monstertacle.SharpQrs.Tests/Core/ResultContractTests.cs` and `InvalidTaskTests.cs`, with dedicated fixtures under `Fixtures/Results/`, enforcing “every actual null result is prohibited, including an empty nullable value type”; cover null reference/nullable-value results, null tasks, non-null nullable values, allowed nullable members, explicit absence results and a request implementing two result contracts to prove wrong-result rejection before invocation (FR-007).
- [X] T038 [P] [US3] Add token/exception probes in `tests/Monstertacle.SharpQrs.Tests/Core/CancellationTests.cs` and `HandlerExceptionTests.cs`, with dedicated fixtures under `Fixtures/Failures/`, covering unchanged tokens, already-cancelled tokens still reaching handlers, cancellation after start, synchronous throws and delayed task faults; enforce “Original synchronous or asynchronous handler exceptions propagate unchanged” (FR-006).
- [X] T039 [P] [US3] Review and rerun the registration-conflict/atomicity tests introduced by T023 in `tests/Monstertacle.SharpQrs.Tests/Integration/RegistrationConflictTests.cs` and `RegistrationAtomicityTests.cs`; add only genuinely missing cases with Red before T042 fixes, cover both request categories without duplicate test copies and document that unbound open types cannot be passed through the closed generic helpers (FR-005, FR-010).
- [X] T040 [P] [US3] Exercise the same input/result/cancellation/exception probes through both integrated dispatchers in `tests/Monstertacle.SharpQrs.Tests/Integration/IntegratedFailureTests.cs`, `IntegratedCancellationTests.cs` and `DependencyResolutionFailureTests.cs`; verify unresolved dependencies propagate without fallback and no candidate handler executes for missing/ambiguous mappings (FR-005–FR-007, FR-009).

### Implementation and Contract Completion

- [X] T041 [US3] Implement only missing core validation in `src/Monstertacle.SharpQrs/DispatchRegistry.cs` and `HandlerRegistration.cs`, ordered as null request, exact mapping/result check, invoke, null task check, await and null result check; enforce “Attempting a duplicate must leave the original entry unchanged”, reject every top-level null and propagate handler failures without a wrapper or cancellation precheck; turn T036–T038 Green.
- [X] T042 [US3] Implement only missing integration conflict/atomicity validation in `src/Monstertacle.SharpQrs.AspNetCore/SharpQrsServiceCollectionExtensions.cs`, `RegisteredHandlerType.cs` and `ScopedDispatcherFactory.cs`; reject conflicts before mutations and preserve helper-owned infrastructure idempotence, turning T039 Green without swallowing service-resolution errors.
- [X] T043 [US3] Turn T040 Green through narrowly scoped fixes in `src/Monstertacle.SharpQrs.AspNetCore/ScopedCommandHandler.cs`, `ScopedQueryHandler.cs` or `ScopedDispatcherFactory.cs`, preserving original tasks, exception/token identity and the model rule “No fallback handler, retry, private provider or provider-construction call is allowed”.
- [X] T044 [US3] Complete exception, cancellation, null-result and registration-timing XML documentation across `src/Monstertacle.SharpQrs/` and `src/Monstertacle.SharpQrs.AspNetCore/`, and explain application-defined non-null absence representations in `examples/Standalone/README.md` and `examples/Web/README.md`; do not introduce a mandatory result-wrapper API (FR-013).
- [X] T045 [US3] Run the entire VSTest suite for both request categories and both usage modes, collect preliminary production-module coverage and full metrics, and inspect untested relevant branches without adding a numerical branch gate; refactor only with passing regression tests and record results in `specs/001-cqrs-libraries/implementation-evidence.md` (SC-003, QC-002–QC-004).
- [X] T046 [US3] Review all Story 3 acceptance scenarios and error precedence against `specs/001-cqrs-libraries/contracts/core-api.md` and `contracts/registration-api.md`, recording scenario-to-test mappings and unresolved findings in `specs/001-cqrs-libraries/implementation-evidence.md`; do not mark the story complete while any required failure check is missing.

**Checkpoint**: All three stories have passing behavioural checks. Packaging, coverage
enforcement, complete metrics accounting, CI and final review still block final acceptance.

## Phase 6: Polish and Cross-Cutting Verification

**Purpose**: Turn behavioural evidence into reproducible, fail-closed acceptance gates.

- [X] T047 [P] Write dependency-free checker Red tests in `tests/Scripts/CheckCoverage.Tests.ps1` with report fixtures under `tests/Scripts/Fixtures/Coverage/`, covering both required modules at/above 90%, one module below 90% despite a passing average, missing module/report, zero executable lines, stale/malformed reports and branch reporting without an extra threshold; establish failure against a minimal checker stub in `scripts/check-coverage.ps1`.
- [X] T048 Implement `scripts/check-coverage.ps1` to turn T047 Green, calculating each expected production assembly separately, rejecting incomplete or stale evidence and preserving non-zero failure; never exclude hand-written production code or accept a repository-wide average (QC-003).
- [X] T049 [P] Write dependency-free metrics-wrapper Red tests in `tests/Scripts/CheckMetrics.Tests.ps1` with fixtures under `tests/Scripts/Fixtures/Metrics/`, covering guard exits 0/1/2, absent tool, empty report, parse errors, omitted hand-written source, inactive/unmeasured partial code and documented external-symbol limitations; use controlled test doubles for subprocess/report cases without presenting them as actual Codemetr evidence.
- [X] T050 Implement `scripts/check-metrics.ps1` to turn T049 Green, invoking pinned Codemetr over all `src`/`tests`/`examples` C# with matching context, preserving its exit status and reconciling eligible reported paths against the actual source inventory; distinguish review findings from tool-enforced verdicts and keep baselines/exclusions prohibited (QC-004).
- [X] T051 Add dependency-free orchestration Red tests in `tests/Scripts/Verify.Tests.ps1`, then implement `scripts/verify.ps1` for locked restore, Release build, VSTest/Coverlet, coverage/metrics gates, example smoke checks, mandatory `scripts/check-packages.ps1` and `scripts/check-audit.ps1` calls, and script-harness checks; use fresh per-run reports, preserve component failures and reject missing scripts; test orchestration with controlled subprocess doubles without recursive self-invocation, and defer a real full run until T054 and T056 exist.
- [ ] T052 Create `.github/workflows/verify.yml` running the same complete `scripts/verify.ps1` on Windows and Linux without package/audit skip flags, with exact SDK provisioning, recorded PowerShell prerequisites, least-privilege permissions and reviewed immutable action revisions; validate configuration now, run the complete hosted workflow only after T054/T056 and authorised Git/remote setup, and do not equate local validation with a hosted CI pass (QC-001–QC-004).
- [X] T053 Finish local package metadata in `src/Monstertacle.SharpQrs/Monstertacle.SharpQrs.csproj` and `src/Monstertacle.SharpQrs.AspNetCore/Monstertacle.SharpQrs.AspNetCore.csproj`, preserving root `LICENSE`, MIT declarations, documentation and compatible core-package dependency; use an explicit initial prerelease package version for local checks and verify metadata without publishing (QC-005).
- [X] T054 Create and execute `scripts/check-packages.ps1` for local packing and temporary consumers outside the source tree, using only the local package feed plus nuget.org; run standalone and integrated request probes without SharpClaw or project references, inspect notices and verify the standalone consumer requires no ASP.NET Core runtime; clean up only script-owned temporary paths and never push a package (FR-001, FR-011, SC-001, SC-004).
- [X] T055 [P] Write root `README.md` and `docs/development.md` covering both usage modes, approved tool setup, metric/review classifications, TDD, ownership, 90% per-library line coverage, branch reporting and pinned skill provenance; align example commands with `specs/001-cqrs-libraries/quickstart.md` and retain required third-party licence notices without copying unlicensed SharpClaw code (FR-012–FR-013, QC-005–QC-006).
- [X] T056 Write dependency-free Red tests in `tests/Scripts/CheckAudit.Tests.ps1` with schema-version-1 fixtures under `tests/Scripts/Fixtures/Audit/`, then implement `scripts/check-audit.ps1` using the two exact commands and fail-closed verdict in `specs/001-cqrs-libraries/plan.md`; reject every reported vulnerability, including zero-exit-code findings, malformed/incomplete/mismatched evidence and unavailable audit data; configure explicit nuget.org audit sources in `NuGet.Config`, reconcile all projects/frameworks and record actual audit results plus separate bundled-tool licence/dependency review in `specs/001-cqrs-libraries/implementation-evidence.md`.
- [ ] T057 Run the complete `scripts/verify.ps1`, including its mandatory package/audit components and all gate-harness checks, on the final candidate revision; repair issues through failing-test-first changes and rerun affected/full checks, recording per-library line/branch coverage, actual guard/audit results and Windows/Linux evidence or explicitly outstanding platform checks in `specs/001-cqrs-libraries/implementation-evidence.md`.
- [X] T058 Review the complete eligible Codemetr report, SOLID, public API/source/binary/behavioural compatibility, absence of reverse framework dependencies, one-type-per-file discipline and British English; record advisory cognitive-complexity/cohesion/cycle findings, analysis limits and justified inapplicability in `specs/001-cqrs-libraries/implementation-evidence.md` instead of treating exit zero as architectural proof.
- [ ] T059 Validate every command and consumer scenario in `specs/001-cqrs-libraries/quickstart.md` from a fresh checkout of an authorised Git commit, proving no sibling inputs or undocumented setup; an isolated source snapshot is interim evidence only and must not complete T059 or SC-004; if Git initialisation/commit permission is absent, request it separately and leave final acceptance blocked rather than silently substituting snapshot evidence (SC-004).
- [ ] T060 Complete the FR/QC/SC acceptance matrix in `specs/001-cqrs-libraries/implementation-evidence.md`, including the fresh-checkout prerequisite and actual versus outstanding gates/approvals; update `specs/001-cqrs-libraries/tasks.md` only for genuinely finished work, and request final acceptance only after every mandatory criterion passes, without automatically committing, publishing or proceeding to another feature.

## Dependencies and Execution Order

### Phase Dependencies

```text
Review tasks -> authorise Analyze -> resolve blocking findings -> authorise implementation
  -> T001 approvals
  -> Setup T002–T007
  -> Foundations T008–T010
  -> US1 T011–T021
  -> US2 T022–T035
  -> US3 T036–T046
  -> Cross-cutting gates T047–T060
  -> user acceptance (not publication)
```

The task file is documentation, not evidence that Analyze, metrics adoption or
installation consent has already happened. T001 must verify these before work starts.

### User Story Dependencies

- **US1** requires only setup/foundations and delivers the standalone MVP.
- **US2** depends on US1's stable core contracts and dispatch behaviour. It can be
  tested independently using real service scopes and TestHost, without the examples.
- **US3** completes the negative contract of both usage modes and therefore depends
  on US1 and US2 for integrated acceptance. Core failure tests may be authored after
  US1 while US2 is in development, but US3 cannot be signed off before integration exists.
- All three are required for final feature acceptance. Their independent tests do
  not mean that their source changes can safely proceed without dependency ordering.

### Within Each Story

- US1: T011/T012 -> T013 -> T014/T015 -> T016/T017 -> T018 -> T019/T020 -> T021.
  T014/T015 share the registry and run sequentially; parallel marking is intentionally absent.
- T011/T012 prepare the first duplicate/null-handler/original-preservation tests.
  T013 introduces only happy-path storage; T014/T015 make valid setup succeed, then
  establish Red at the intended guard assertion before implementing that validation.
  A failure while arranging the initial valid registration is not a duplicate-guard Red.
- US2: T022 -> test-first pairs from T023–T026 -> T027–T031 -> T032 -> T033–T035.
  Do not wait for every test family to fail before completing one minimal slice.
- T023 includes the first conflict and atomicity tests, which precede any validation
  in T027/T028. T039 extends or reruns those checks rather than introducing them late.
- US3: independent test families T036–T040 -> corresponding production fixes
  T041–T043 -> documentation T044 -> verification T045 -> review T046.
- Common gate pairs: T047 -> T048; T049 -> T050; then T051 -> T052.
  T051/T052 initially validate orchestration/configuration, not a live full run.
  T053 precedes T054; T056 adds the audit gate through its own Red/Green cycle.
  T057's real complete entry-point run waits for T054 and T056;
  T058/T059 evaluate the verified candidate and T060 records final acceptance status.
- Shared evidence-file updates, solution/project changes, builds and production
  registry/helper edits must be serialised within this workspace.

## Parallel Opportunities and Examples

Parallelism is optional. Only independent file authoring is offered here; tests and
builds sharing output directories run serially, and no extra agent is implicitly required.

### User Story 1

After T010, T011 command tests and T012 query tests may be authored independently.
After T015, T016 concurrency tests and T017 ownership/isolation tests may be authored
independently. Run each Red check serially before its associated production refinement.

### User Story 2

After T022, independent files/fixture directories allow T023 registration tests,
T024 scope tests, T025 disposal tests and T026 HTTP tests to be authored separately.
All depend on the common public stubs; registration, factory and adapter implementation
is then integrated serially rather than modified by competing writers.

### User Story 3

After US2, T036 registration/input failures, T037 result/task failures, T038 token/error
behaviour, T039 registration conflicts and T040 integrated failures have distinct test
files. Their production fixes touch shared registry/helpers and remain serial.

### Cross-Cutting Work

T047 and T049 checker-test authoring use separate scripts and fixture directories.
T055 prose can be drafted after all story contracts stabilise, alongside gate work,
but final commands must be reconciled after scripts exist. No `[P]` marking permits
concurrent changes to `Monstertacle.SharpQrs.slnx`, central versions or evidence files.

## Requirement Coverage

| Requirements | Primary tasks |
| --- | --- |
| FR-001 | T004, T019, T054 |
| FR-002 | T008, T011–T015 |
| FR-003 | T011–T015, T020 |
| FR-004 | T013–T018, T032 |
| FR-005 | T011–T014, T023, T028, T036, T039–T042, T046 |
| FR-006 | T038, T040–T043 |
| FR-007 | T011–T014, T023, T028, T036–T037, T040–T043 |
| FR-008 | T022–T024, T027–T030 |
| FR-009 | T024–T026, T029–T032, T040 |
| FR-010 | T023, T028, T039, T042 |
| FR-011 | T004, T019, T054, T059 |
| FR-012 | T020–T021, T033–T035, T055, T059 |
| FR-013 | T021, T034, T044, T055, T058 |
| FR-014 | T017–T018, T020, T025, T031 |
| FR-015 | T017–T018, T027, T032 |
| QC-001 | T002–T006, T052, T057 |
| QC-002 | Test-first story pairs throughout; T045, T058 |
| QC-003 | T045, T047–T048, T051, T057 |
| QC-004 | T007, T014, T035, T045, T049–T051, T057–T058 |
| QC-005 | T004, T044, T053, T055–T056, T058 |
| QC-006 | T001, T007, T051, T055, T060 |
| SC-001 | T020, T054 |
| SC-002 | T024–T026, T031, T035 |
| SC-003 | T036–T046 |
| SC-004 | T021, T034, T054–T055, T059 |
| SC-005 | T016–T018, T025, T031–T032, T057 |

## Implementation Strategy

### MVP First

Complete setup, foundations and US1, then demonstrate only the core through its
standalone consumer. Stop at T021 for an observable checkpoint. This is not permission
to omit US2, US3 or constitutional quality gates from the finished feature.

### Incremental Delivery

1. Review this task list and explicitly authorise `/speckit.analyze`.
2. Resolve analysis findings and obtain implementation/metrics/tool approvals.
3. Implement core behaviour in small Red/Green slices; record first-slice metrics.
4. Add real scoped integration and verify disposal before expanding its surface.
5. Complete failure/cancellation/null contracts across both usage modes.
6. Finish reproducible gates, local package consumers, documentation and final review;
   obtain separately authorised Git setup and complete fresh-checkout verification
   before final acceptance. A snapshot does not satisfy that requirement.
7. Request acceptance. Publication, Git initialisation and commits remain separate actions.

## Notes

- There are 60 tasks: 7 setup, 3 foundational, 11 US1, 14 US2, 11 US3 and 14 final tasks.
- The 16 `[P]` tasks identify file-independent authoring opportunities subject to the
  dependency graph. They are not a claim that all marked tasks can start together.
- No rate limits, persistence, resultless variants, scanning, middleware, performance
  benchmark targets or production HTTP endpoint generation are introduced.
- Missing approvals, metrics capability gaps, security findings or unavailable
  platform/CI evidence must remain visible; never check off a task by assuming success.
