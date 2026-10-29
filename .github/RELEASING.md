# Manual NuGet releases

Releases follow the same manual publishing pattern as Codemetr.CSharp: choose
**Actions > release > Run workflow**, select `main`, and supply a NuGet version.
The policy matches `.github/workflows/release.yml` and environment `nuget`.

## One-time setup

The configured publishing identity is the personal NuGet profile `bleissem`, stored
as the GitHub environment secret `NUGET_USER`. The policy owner is the account or
organisation selected on nuget.org; the login identity remains the personal profile.
That identity needs publishing permissions for the selected owner.

The NuGet Trusted Publishing policy uses:

| Field | Value |
| --- | --- |
| Repository owner | `monstertacle` |
| Repository | `monstertacle.sharpqrs` |
| Workflow file | `release.yml` |
| Environment | `nuget` |
| Package glob | `Monstertacle.SharpQrs*` |
| Scopes | Push new packages and new versions |

The GitHub environment `nuget` permits deployments from `main`. Login uses the
same pinned `NuGet/login` revision as the reference workflow and obtains a temporary
credential immediately before publication. Persistent API-key secrets are unnecessary.
See [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Verify a release without publishing

1. Open **Actions > release > Run workflow**.
2. Select branch **main** and enter a version such as `0.1.0-alpha.1`.
3. Enable **dry_run** and run the workflow.

Both Windows and Linux execute the complete shared verification entry point,
including the requested package version, coverage, metrics, dependency audit,
examples and isolated consumers of both packages. The final job downloads the
Linux verification artefact and checks its exact commit/version and clean successful
run. It selects exactly the two expected packages, core first, and reports their
SHA-256 hashes. The dry run skips OIDC login and NuGet pushes; it does not prove
that the external NuGet policy will authorise publication.

## Publish

After the intended changes and checks have landed on `main`, run the same workflow
with the desired version and **dry_run disabled**. This manual action publishes:

- `Monstertacle.SharpQrs`
- `Monstertacle.SharpQrs.AspNetCore`

The supplied version overrides restore, build and pack consistently. Local consumers
verify the integration package's dependency on that same core version. A release
uploads those already verified packages; it does not rebuild them after verification.
Only the publishing job has `id-token: write`, and its environment is `nuget`.
Publication is manual, rather than triggered by a PR, merge, tag or GitHub release.

Use a new version for each publication. Duplicate pushes fail; they are not silently
skipped. If a two-package publication stops after the first upload, inspect the actual
NuGet state before deciding how to finish that version. NuGet has no transactional
multi-package push.

Version inputs accept forms such as `0.1.0` and `0.2.0-rc.1`, without a `v` prefix
or build metadata. Default CI still uses the matching versions in the two projects.
The first planned prerelease is `0.1.0-alpha.1`.

After indexing, consumers can install the integration with:

```bash
dotnet add package Monstertacle.SharpQrs.AspNetCore --version 0.1.0-alpha.1
```

The core is a package dependency. Standalone consumers can install
`Monstertacle.SharpQrs` directly at the same version.
