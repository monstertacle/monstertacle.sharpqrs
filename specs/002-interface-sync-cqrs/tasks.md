# Tasks: Interface Composition and Synchronous CQRS

## Setup and foundation

- [x] T001 Record autonomous authority, clarification and design in specs/002-interface-sync-cqrs/spec.md and plan.md (all requirements).
- [x] T002 Install pinned coverage tool in .config/dotnet-tools.json; verify doctor and skill provenance in specs/002-interface-sync-cqrs/research.md (FR-011, FR-012).

## User Story 1 — interface composition

Independent check: real DI scope resolves an explicitly selected interface service;
standalone configuration uses registration contracts. MVP: async contract composition.

- [x] T003 [US1] Add compilable interface composition stubs and failing tests in tests/Monstertacle.SharpQrs.Tests/Integration/ContractMappingTests.cs (FR-001, FR-002, FR-008).
- [x] T004 [US1] Implement setup interfaces in src/Monstertacle.SharpQrs/ICommandRegistration.cs and IQueryRegistration.cs; add interface mapping helpers and lazy adapters in src/Monstertacle.SharpQrs.AspNetCore (FR-001, FR-002, FR-008).
- [x] T005 [US1] Add negative, decorator and lifetime coverage in tests/Monstertacle.SharpQrs.Tests/Integration/ContractLifetimeTests.cs and ContractValidationTests.cs (FR-001, FR-007, FR-008).

## User Story 2 — synchronous execution

Independent check: direct command/query results and original failures in both compositions.

- [x] T006 [US2] Add compilable sync stubs and executed failing tests in tests/Monstertacle.SharpQrs.Tests/Core/SynchronousDispatchTests.cs (FR-003, FR-006).
- [x] T007 [US2] Implement synchronous core contracts and dispatch in src/Monstertacle.SharpQrs/SyncCommandDispatcher.cs, SyncQueryDispatcher.cs and SyncDispatchRegistry.cs (FR-003, FR-006).
- [x] T008 [US2] Execute failing scoped sync tests then implement integration in src/Monstertacle.SharpQrs.AspNetCore/SharpQrsSynchronousExtensions.cs (FR-003, FR-007, FR-008).
- [x] T009 [US2] Extend edge, token, error and disposal tests in tests/Monstertacle.SharpQrs.Tests/Core/SynchronousContractTests.cs and Integration/SynchronousScopeTests.cs (FR-003, FR-006, FR-008).

## User Story 3 — independent execution modes

Independent check: one request in each mode routes separately, including concurrent calls.

- [x] T010 [US3] Add mixed and wrong-mode checks in tests/Monstertacle.SharpQrs.Tests/Integration/MixedModeTests.cs and Core/SynchronousConcurrencyTests.cs (FR-004, FR-007).
- [x] T011 [US3] Verify published binary and source consumers in scripts/check-compatibility.ps1 including third-party implementations of existing interface contracts (FR-005).

## User Story 4 — mandatory coverage

Independent check: complete same-run coverage and TRX pass; failed or incomplete evidence fails.

- [x] T012 [US4] Write and execute failing evidence tests in tests/Scripts/CheckCover.Tests.ps1 before implementing scripts/check-cover.ps1 (FR-012, FR-013, FR-014).
- [x] T013 [US4] Implement complete coverage verification in scripts/check-cover.ps1 and codemetr-cover.json; preserve independent per-library checker (FR-012, FR-013, FR-014).
- [x] T014 [US4] Wire coverage and compatibility into scripts/verify.ps1 and history/base selection into .github/workflows/verify.yml (FR-012, FR-013, FR-014).

## Polish and acceptance

- [x] T015 Update README.md, docs/development.md and examples/Standalone and examples/Web to demonstrate both modes and interface mappings (FR-009, SC-001–SC-005).
- [x] T016 Run complete verification, review SOLID, compatibility, advisory metrics, licences and language; record evidence in specs/002-interface-sync-cqrs/implementation-evidence.md (FR-010, SC-001–SC-007).
- [x] T017 Commit/push checked changes, open and complete PR after Windows/Linux checks; record final disposition in specs/002-interface-sync-cqrs/plan.md (all requirements).

## Dependencies and execution

T001–T002 precede implementation. Each story runs tests before its implementation.
US1 and US2 are independently testable; US3 follows both. US4 can be developed
independently but the final full check follows all stories. Documentation follows
stable contracts; final checks precede PR completion. Tasks are run sequentially in
this session to avoid editing the same integration and verification files concurrently.
Independent test files could be authored in parallel after their contract signatures
exist, but no parallel agent work is needed. No release is included.
