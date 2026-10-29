# Tasks for DI composition

## Specification and design

- [x] T001 Specify findings and proposed checked contract in specs/003-di-composition/spec.md and contracts/composition.md (FR-001–FR-009).
- [x] T002 Record the owner's boundary decision and complete Clarify/Plan/Analyze in specs/003-di-composition (FR-002–FR-006).

## User story 1 independent mode composition

- [x] T003 [US1] Execute failing mode isolation tests in tests/Monstertacle.SharpQrs.Tests/Integration/ModeIsolationTests.cs (FR-001).
- [x] T004 [US1] Separate shared concrete validation in src/Monstertacle.SharpQrs.AspNetCore/ConcreteHandlerRegistration.cs and both infrastructure validators (FR-001, FR-007).

## User story 2 actual composition validation

- [x] T005 [US2] After T002, add compilable preparation/factory stubs and failing copied-composition tests under tests/Monstertacle.SharpQrs.Tests/Integration (FR-002–FR-005).
- [x] T006 [US2] Implement immutable selection requirements, owned descriptor identity and checked preparation under src/Monstertacle.SharpQrs.AspNetCore (FR-002–FR-005).
- [x] T007 [US2] Implement the provider factory and explicit unprepared-contract diagnostic; update checked test composition while retaining published legacy checks (FR-003–FR-007).
- [x] T008 [US2] Complete conflict, keyed, independent-provider, failure-atomicity and lifetime regressions under tests/Monstertacle.SharpQrs.Tests/Integration (FR-002–FR-007).

## User story 3 safe practical use

- [x] T009 [US3] Execute failing decorated/worker example assertions then implement examples within examples/Web (FR-007, FR-008).
- [x] T010 [US3] Update README.md, examples/Web/README.md and docs/development.md with approved migration and ownership recipes (FR-006, FR-008).

## Verification and delivery

- [x] T011 Run full scripts/verify.ps1, review compatibility/SOLID/advisory metrics and record evidence in specs/003-di-composition/implementation-evidence.md (FR-009).
- [ ] T012 Resolve all four origin findings in docs/plans/di-composition-hardening.md; commit/push checked changes and complete the verified PR (FR-009).

T003–T004 are independent of the boundary decision. T005–T008 and the checked-host migration in T010 require T002. T009 can first
exercise the already-approved concrete helper path independently.
Each behaviour is tested against the old implementation or compilable stubs first.
T011–T012 require all preceding work; no unchecked decision is represented as acceptance.
