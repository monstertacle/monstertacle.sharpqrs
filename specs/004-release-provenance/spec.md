# Feature Specification: Release record for the published 0.2.0-alpha.1 packages

**Created**: 2026-11-02
**Status**: Specified, clarified, planned and analysed; implemented as documentation only
**Authority**: Owner instruction of 2026-11-02 to implement this release-provenance improvement end to end, including the
staged Spec Kit phases, a pull request and merge once all checks pass. No publication, version change or package rebuild is
authorised or needed.

## Problem

`Monstertacle.SharpQrs` and `Monstertacle.SharpQrs.AspNetCore` 0.2.0-alpha.1 are published on nuget.org, and the core package
is consumed downstream with a locked content hash. The repository records how releases are set up
(`.github/RELEASING.md`, `docs/nuget-release-evidence.md`), but no committed file names the producer run, source commit or
package hashes of any actual publication. The project versions stay at `0.1.0-alpha.1` because the release workflow
supplies the version as an input; that is intended, but it means the published version cannot be traced from the source
tree alone. The verification artefact holding the producer bytes expires on 2026-11-12T22:01:42Z, after which the chain
could only be reconstructed from workflow logs, which are also subject to retention.

## User Scenarios and Testing

### Story 1 — Trace a consumed package to its producer (P1)

A consumer holding a lock file entry for `Monstertacle.SharpQrs` 0.2.0-alpha.1 can find, in this repository, the source
commit, release workflow run, producer artefact and package hashes, and can repeat each comparison from public sources.
Acceptance: every value in the record was compared against its public source on the recording date; the steps to repeat
the comparisons are given.

### Story 2 — Distinguish verified facts from unknowns (P1)

A reviewer can see what the record establishes and what it does not, without mistaking an observational record for a signed
build attestation. Acceptance: the record states its status and lists each fact that was not established.

## Requirements

- **FR-001**: A committed release record for 0.2.0-alpha.1 identifies the source commit, the release workflow run, its jobs,
  the version input, the producer artefact and the package files with their SHA-256 hashes.
- **FR-002**: The record distinguishes the canonical producer package (unsigned, as built by the verification job) from the
  package served by nuget.org (the same entries plus the repository signature entry) and gives both SHA-256 hashes, the
  entry comparison result and the repository signature subject.
- **FR-003**: The record gives the NuGet content hash (SHA-512, Base64) of each canonical package, which is the value a
  locked consumer stores.
- **FR-004**: The record lists what it does not establish, including reproducible rebuild equivalence, build attestation,
  author signing, server-side publishing policy and package-ID prefix verification.
- **FR-005**: `.github/RELEASING.md` tells maintainers to add such a record after each publication and states which earlier
  publication has no record.
- **FR-006**: No library source, project version, package byte, workflow, script or quality threshold changes.

## Success Criteria

- **SC-001**: Each recorded hash, identifier and time matches the public source it names (checked on 2026-11-02).
- **SC-002**: The full verification script and hosted CI on Linux and Windows pass on the change.
- **SC-003**: The diff touches documentation and specification files only.

## Clarifications

### Session 2026-11-02

- Q: Should the record be machine-readable with a new checker? → A: No. Nothing in this repository consumes such a record, and
  a checker could only test the format of text written for it. A plain record with reproducible steps is the smallest useful
  change (Constitution I; III permits documentation-only changes without artificial tests).
- Q: Should 0.1.0-alpha.1 also be recorded? → A: Not in this feature. No observed consumer uses it; its served packages were
  not compared. RELEASING.md states that it has no record.
- Q: Does the project default of `0.1.0-alpha.1` need changing? → A: No. The workflow version input is the documented design.
- No further questions remain.
