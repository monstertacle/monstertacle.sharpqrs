# Tasks: .NET 11 support

## Phase 1: Setup

- [X] T001 Ratify approved amendment in .specify/memory/constitution.md and record phase authority in specs/004-dotnet11-support/plan.md.
- [X] T002 Establish clean baseline and install pinned SDKs described in specs/004-dotnet11-support/research.md.

## Phase 2: Foundation

- [X] T003 Execute new dual-target script acceptance cases before implementing verifier behaviour in tests/Scripts/CheckAudit.Tests.ps1 and ReleasePackages.Tests.ps1 (FR-004, FR-007).

## Phase 3: US1 - Adopt the new runtime

- [X] T004 [US1] Add both target frameworks to src/, examples/ and tests/ csproj files; pin SDK and conditional Microsoft dependencies in global.json and Directory.Packages.props; regenerate packages.lock.json files (FR-001, FR-006).
- [X] T005 [US1] Build and execute unchanged contract tests on net11.0 via scripts/verify.ps1 (FR-001, FR-003).

## Phase 4: US2 - Retain existing consumers

- [X] T006 [US2] Preserve and execute net10.0 tests and scripts/check-compatibility.ps1 unchanged against published assemblies (FR-002).

## Phase 5: US3 - Verify and package both targets

- [X] T007 [US3] Isolate tests and both coverage checks by framework in scripts/verify.ps1; parameterise scripts/check-web.ps1 and execute every example for both targets (FR-003).
- [X] T008 [US3] Validate both audit inventories and per-target locks in scripts/check-audit.ps1; exercise missing, duplicate and second-target vulnerability cases (FR-004).
- [X] T009 [US3] Validate both asset/dependency groups and run four isolated consumers in scripts/check-packages.ps1 (FR-005).
- [X] T010 [US3] Require dual-target release evidence in scripts/release-packages.ps1 and update .github/workflows/verify.yml SDK setup (FR-006, FR-007).

## Phase 6: Polish and acceptance

- [X] T011 Update README.md and docs/development.md; run complete scripts/verify.ps1 and record results and final review in specs/004-dotnet11-support/implementation-evidence.md (FR-001–FR-007, SC-001–SC-003).

## Dependencies and execution

T001 -> T002 -> T003 -> T004 -> T005/T006 -> T007–T010 -> T011.
US1 is the adoption increment; US2 independently verifies retention. US3 combines
both into release qualification. Shared scripts make sequential execution safer;
no parallel writer tasks are planned. Read-only SDK and script research may run
alongside setup. Build/configuration changes use existing runtime tests; new script
validation receives Red/Green checks without artificial domain tests.
