# Feature Specification: .NET 11 support

**Feature Branch**: `feature/004-dotnet11-support`
**Created**: 2026-11-14T08:33:04Z (measured with `date -u`)
**Status**: Ready for implementation under the owner's autonomous phase approval.
**Input**: Realise Monstertacle.SharpQrs for .NET 11 using Spec Kit.

## User Scenarios & Testing

### User Story 1 - Adopt the new runtime (Priority: P1)

As a library consumer, I can use standalone and scoped command/query dispatch in
.NET 11 applications with the documented semantics.

**Why this priority**: This is the requested capability.
**Independent Test**: Execute the existing contract suite and standalone, HTTP and
worker examples on the new runtime, then consume the packed libraries externally.

**Acceptance Scenarios**:

1. **Given** a .NET 11 application, **When** it consumes either library, **Then**
   synchronous and asynchronous dispatch, cancellation, errors and scoped ownership
   behave according to the existing contracts.
2. **Given** a standalone consumer, **When** it runs, **Then** ASP.NET Core is not required.

### User Story 2 - Retain existing consumers (Priority: P1)

As an existing consumer, I can continue using .NET 10 without changing my code.

**Why this priority**: Adding support must preserve published compatibility.
**Independent Test**: Run the original compiled published consumer against the
candidate assemblies and recompile its unchanged source; execute existing tests.

**Acceptance Scenarios**:

1. **Given** the published baseline consumer, **When** candidate assemblies replace
   the originals, **Then** the binary runs and the unchanged source recompiles.

### User Story 3 - Verify and package both targets (Priority: P2)

As a maintainer, I can run one reproducible verification procedure and see separate
results for both supported runtimes before releasing the two library packages.

**Why this priority**: Successful compilation alone does not establish support.
**Independent Test**: Run the complete verifier and inspect its reports and packages.

**Acceptance Scenarios**:

1. **Given** either target fails verification, **When** verification finishes,
   **Then** it cannot report complete success or qualify packages for release.
2. **Given** verified packages, **When** either runtime restores them, **Then** it
   selects its matching assets and framework-compatible integration dependencies.

### Edge Cases

Missing runtime, duplicate/missing audit targets, vulnerabilities in either target,
absent package assets, failing/skipped tests and incomplete release evidence fail
explicitly. Coverage evidence for one target cannot substitute for the other.

## Requirements

### Functional Requirements

- **FR-001**: Both libraries, tests and examples MUST support .NET 10 and .NET 11.
- **FR-002**: Existing public API, behaviour and published .NET 10 source/binary
  compatibility MUST be preserved; the standalone library MUST stay dependency-free.
- **FR-003**: Verification MUST execute the complete behavioural suite, per-library
  90% line coverage gates and complete Codemetr coverage checks separately per target.
- **FR-004**: Audit MUST validate both complete target inventories against lock files
  and reject every reported vulnerability, missing target or malformed evidence.
- **FR-005**: Each of the two packages MUST contain both target assets; standalone
  and scoped external package consumers MUST execute on each supported runtime.
- **FR-006**: SDK, dependencies and CI MUST be reproducible; CI MUST retain Windows
  and Linux verification, with no weakening of metrics or coverage policy.
- **FR-007**: Release selection MUST require successful evidence for both targets.
  Documentation MUST give the actual setup and verification commands.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All existing behavioural tests pass with zero failures or skips on
  each supported runtime; all four standalone/integrated package consumer runs pass.
- **SC-002**: Each library meets the existing 90% line floor independently on each
  runtime; complete coverage, strict metrics and dependency audit gates pass.
- **SC-003**: Both candidate packages contain exactly the two supported runtime asset
  groups; the original published consumer passes binary and source checks.

## Assumptions

The owner's approval includes the proposed retention of .NET 10 and autonomous
phase progression. No new CQRS behaviour, API, package publication or performance
claim is requested. Existing C# language/analyser settings and numerical rules stay
in force. Network access to official SDK sources and nuget.org is available.

## Clarifications

### Session 2026-11-14

No critical ambiguities remain. The owner approved the concrete dual-target
constitution amendment and autonomous execution. No additional questions asked.
