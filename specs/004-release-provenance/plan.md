# Implementation Plan: Release record for 0.2.0-alpha.1

**Spec**: specs/004-release-provenance/spec.md | **Constitution**: 2.0.0

## Approach

Collect each fact from its public source, compare it with the others and write one release record:

| Fact | Source |
| --- | --- |
| Run, attempt, event, branch, source commit, jobs | GitHub Actions API for the `release` workflow |
| Version input and pushed files | publish job log (`PACKAGE_VERSION`, inspection hashes, push results) |
| Producer bytes and verification status | artefact `verification-ubuntu-24.04` of the same run (`run.json`, `packages/`) |
| Served bytes and signature | nuget.org flat container download; `dotnet nuget verify --all` |
| Embedded identity | `.nuspec` in each canonical package |
| Content hash | SHA-512 of each canonical package, compared with a downstream lock file |

Record location: `docs/releases/0.2.0-alpha.1.md`. `.github/RELEASING.md` gains a short section linking to it.

## Decisions

- **Documentation only.** Alternatives considered: a JSON record with a validation script (rejected: no consumer in this
  repository, and offline validation would only check its own format); republishing or rebuilding packages (rejected:
  published bytes must not change, and a rebuild proves nothing about the published bytes).
- **Observational status.** The record is labelled as checked public facts, not as a signed attestation; no attestation exists.
- **No network binaries executed.** Downloaded packages are only hashed, unzipped for comparison and verified with the pinned SDK.

## Verification

Full `scripts/verify.ps1` locally (capped resources) and hosted Verify on Linux and Windows. No new behaviour, so no Red phase
is required (Constitution III); the factual comparisons are recorded in implementation-evidence.md.

## Constitution check

I: no new feature in the libraries. II–IV: no C# change; metrics and coverage gates run unchanged. V: phases recorded here and
in tasks.md under the owner's end-to-end authority. VI: British English.
