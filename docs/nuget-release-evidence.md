# NuGet release setup evidence

Updated: 2026-10-29T17:26:24Z

## Scope and authorisation

The user configured the NuGet Trusted Publishing policy, supplied personal profile
`bleissem`, and requested the same release approach as Codemetr. This implements
workflow/environment setup and verification. First public package publication
remains a separate owner action; no upload is part of setup validation.

The reference is `codemetr/Codemetr.CSharp` at local revision
`1cbcae66eef2af5ea58c77f1e632036440442ee5`, specifically `.github/RELEASING.md` and
`.github/workflows/release.yml`. Its manual version input, main-only publishing,
`nuget` environment, personal `NUGET_USER` identity and pinned OIDC login are retained.
SharpQrs keeps its existing immutable build-action pins and full Windows/Linux gates;
it publishes both libraries rather than the reference's single tool package.

## Changes and checks

- Added manual `release.yml` with version input and optional dry run; no automatic
  publication on a PR, merge, tag or GitHub release.
- Reused the existing complete verifier through `workflow_call`, with no reduced
  release-only check set. Only the final publishing job requests OIDC permissions.
- Version override is propagated through restore, build, tests, pack, metadata checks
  and both isolated package consumers. Default CI still resolves matching project versions.
- The publishing job downloads this same run's Linux artefact, requires exact source
  SHA/version and a clean successful run, and selects exactly the two expected packages.
- NuGet authentication uses `NuGet/login` commit
  `8d196754b4036150537f80ac539e15c2f1028841`, matching the reference. New artefact download
  action is pinned to `d3f86a106a0bac45b974a628896c90dbdf5c8093`.
- Configured GitHub environment `nuget`, restricted to branch `main`, and environment
  secret `NUGET_USER` with the supplied public profile name. No persistent NuGet API key.

Package-version tests executed Red against a callable stub: invalid versions were
accepted, matching project defaults were not resolved and mismatched defaults were
accepted. The implementation then made all cases Green, including numeric prerelease
leading zeroes and malformed input. Artefact-selection tests executed Red for wrong
source/version, incomplete/dirty runs, unexpected/missing packages and core-first
selection, then passed after implementing the guard. These dependency-free tests are
mandatory in the shared verification script. Controlled selection fixtures are not
claimed as actual NuGet package validation.

Both workflow files parse as YAML. Real local verification runs follow below; hosted
CI, the reviewable PR and the main-branch workflow dry run remain setup completion gates.
The dry run deliberately skips external authentication/push, so NuGet policy acceptance
is not claimed until an owner-authorised publication occurs.

## Local verification

- `verify-20261029T171910Z-849204c838404dbc91d56d9b6f993f83`: version `0.2.0-rc.1`, 87/87 tests passed; full verifier status `passed`. Base revision `a68b19d0b74f8454645ff128289fbe10e747e62e` with the setup working tree.
- `verify-20261029T172225Z-bbf11cda764044fba6ed881b1bed1fa5`: version `0.1.0-alpha.1`, 87/87 tests passed; full verifier status `passed`. Base revision `a68b19d0b74f8454645ff128289fbe10e747e62e` with the setup working tree.

Production C# sources, public API, dependency versions and quality thresholds are unchanged.
