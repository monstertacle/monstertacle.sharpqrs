# Implementation evidence: 0.2.0-alpha.1 release record

**Base**: `main` at `7dcef4698badc2f16109e39b6527d1da970fc532`. The libraries are unchanged since the producer commit
`554f4a02f23d20c2e0012d3420ff58f7370b4a62` (`git diff --stat 554f4a0 7dcef46 -- src` is empty).

## Premise

- No committed file named `0.2.0-alpha.1`, run 40962417646 or the producer commit before this change (`git grep`).
- `docs/nuget-release-evidence.md` was last updated at 2026-10-29T17:26:24Z, before either publication. It records the
  setup and local verifier runs, not a published release.
- The release workflow lists three runs: 40922037100 (dry run, `0.1.0-alpha.1` setup check), 40924045518 (published
  `0.1.0-alpha.1` from `b1535a0d265230201dea0aed2cd7c52d1bf13f11`) and 40962417646 (published `0.2.0-alpha.1`).

## Cross-checks performed on 2026-11-02

| Check | Result |
| --- | --- |
| Run metadata (API) against `run.json` revision and version | equal |
| Publish log inspection hashes against artefact `packages/*.nupkg` SHA-256 | equal for both packages |
| Canonical against served entries (extracted, recursive comparison) | only `.signature.p7s` added |
| `dotnet nuget verify --all` (SDK 10.0.403) on both served packages | exit 0; repository signature |
| `.nuspec` repository commit | equals the producer commit |
| Base64 SHA-512 of the canonical core package against the downstream lock entry | equal |
| GitHub attestation lookup for the canonical core digest | HTTP 404, none |
| nuget.org search `verified` for `Monstertacle.SharpQrs` | `false` |

No package was uploaded, no workflow was dispatched and no downloaded binary was executed.

## Verification of this change

The diff adds documentation and specification files and edits `.github/RELEASING.md` only; library sources, projects,
scripts, workflows and quality configuration are unchanged. Results of `scripts/verify.ps1` and hosted Verify are recorded
in the pull request.

Local `scripts/verify.ps1` (2026-11-02T20:18:36Z–20:19:20Z, SDK 10.0.403, PowerShell 7.6.9, base `7dcef46` with this
working tree): status `passed`, 187/187 tests, all stages including coverage, metrics, audit and package consumers. Hosted
Verify on Linux and Windows runs on the pull request.
