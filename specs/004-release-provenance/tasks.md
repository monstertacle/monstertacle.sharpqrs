# Tasks for the 0.2.0-alpha.1 release record

- [x] T001 Specify and clarify in specs/004-release-provenance/spec.md (FR-001–FR-006).
- [x] T002 Plan sources and decisions in specs/004-release-provenance/plan.md (FR-001–FR-006).
- [x] T003 Collect and cross-check the public facts; record them in specs/004-release-provenance/implementation-evidence.md (SC-001).
- [x] T004 Write docs/releases/0.2.0-alpha.1.md (FR-001–FR-004).
- [x] T005 Add the release record section to .github/RELEASING.md (FR-005).
- [x] T006 Analyse requirements, plan, tasks and Constitution for consistency (below).
- [x] T007 Run scripts/verify.ps1 locally and hosted Verify on Linux and Windows; confirm the diff is documentation only (SC-002, SC-003, FR-006).

## Analysis

Every requirement maps to a task: FR-001–FR-004 → T003, T004; FR-005 → T005; FR-006 → T007. No task adds library behaviour,
so the Constitution's Red-first rule does not apply (III, documentation-only clause); no threshold or tool pin changes (IV).
No contradiction remains between the specification, plan and tasks.
