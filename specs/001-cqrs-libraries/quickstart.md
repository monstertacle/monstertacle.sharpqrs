# Quickstart Validation Guide

**Feature**: `001-cqrs-libraries`

This is a prospective guide for the implementation phase. The solution, examples,
scripts and manifests named below do not exist yet. No command below has been
executed against an implementation; planning does not authorise their installation
or create an application. Run them only after those artefacts are implemented.

## Prerequisites

- .NET SDK 10.0.403 and a compatible .NET 10 runtime; ASP.NET Core 10 runtime for
  the web example. The future `global.json` requires this exact SDK.
- PowerShell 7 on Windows or Linux.
- Access to nuget.org for locked package restore and the published tool restore.
- User approval for the plan, metric profile and required repository-local tooling.

The planned tool manifest pins `Codemetr.CSharp.Tool` 0.1.0. Do not install a global
tool, fetch a sibling repository build or silently substitute a newer version.

## Restore and Build

From the repository root, after implementation and installation approval:

```powershell
dotnet --version
dotnet tool restore
dotnet restore Monstertacle.SharpQrs.slnx --locked-mode
dotnet build Monstertacle.SharpQrs.slnx --configuration Release --no-restore
```

Expected: exact SDK selected, all projects restored from committed lock files and
a warning-free Release build. Initial lock-file generation belongs to implementation;
do not use locked restore before those files exist.

## Full Verification

```powershell
pwsh -NoProfile -File scripts/verify.ps1
```

The planned entry point performs the same checks locally and in Windows/Linux CI:
locked restore, Release build, VSTest execution, fresh coverage reports, coverage
validation, full metrics, dependency audit, executable example smoke checks and
isolated package-consumer checks. It invokes both `scripts/check-audit.ps1` and
`scripts/check-packages.ps1`; CI must not omit either check.
It fails on any non-zero mandatory subprocess result; reports identify source
revision, tool versions and scope. Manual review items are reported separately,
never presented as passing merely because the script exits zero.

The audit gate parses versioned dependency and vulnerability JSON plus diagnostics.
Any reported vulnerability, missing/incomplete evidence or failure to obtain audit
data fails verification, even when the listing command itself returns zero. See
[the audit verdict](plan.md#dependency-audit-verdict) for the full policy.

### Inspect Individual Checks

```powershell
dotnet test tests/Monstertacle.SharpQrs.Tests/Monstertacle.SharpQrs.Tests.csproj --configuration Release --no-build --no-restore
dotnet tool run codemetr-csharp check src tests examples --include-metrics --format json
```

These are the proposed VSTest and full-scope guard forms, not evidence of success.
The coverage wrapper supplies Coverlet properties and a fresh report path; use the
full entry point for acceptance rather than a test run without coverage.
`analyze` alone is not a guard and changed-file checks cannot replace a full check.

## Acceptance Scenarios

### 1. Standalone Consumer

```powershell
dotnet run --project examples/Standalone --configuration Release --no-build
```

Expected: one explicit command handler and query handler return their expected
non-null results; the example exits successfully without the integration library
or a web host. Ownership remains with the caller. Covers SC-001 and FR-014.

### 2. Web Consumer

```powershell
dotnet run --project examples/Web --configuration Release --no-build -- --urls http://127.0.0.1:5080
```

In a separate terminal, invoke the planned demonstration endpoints:

```powershell
Invoke-RestMethod -Method Post -Uri http://127.0.0.1:5080/demo/commands -ContentType application/json -Body '{"value":"example"}'
Invoke-RestMethod -Method Get -Uri http://127.0.0.1:5080/demo/queries/example
```

Expected: documented non-null demonstration results and distinct request-scope
identities between requests. Automated TestHost checks prove reuse within a request
and exactly-once disposal after completion. Bind the example to loopback only;
it is not a production or public control service. Stop it after validation.

These are example endpoints, not generated library endpoints or a new production
HTTP contract. Handler internals belong to implementation, not this guide.

### 3. Invalid Calls and Isolation

The test suite must demonstrate missing/duplicate handlers, exact type and result
matching, null inputs/tasks/results, original exception and token propagation,
cancelled operations and concurrent routing. It must prove independent registrations
between dispatcher instances and providers. See the two [contracts](contracts/core-api.md)
and [data model](data-model.md) for precise expected behaviour.

### 4. Coverage and Metrics

Expected fresh evidence: both production assemblies present with executable line
counts and at least 90% line coverage each; branch results reported for review.
The coverage checker must reject a missing assembly or report rather than accept
an average. Metrics cover all hand-written C# in source, tests and examples, including
partial-type aggregation. Review cognitive complexity, cohesion, cycles and analysis
limitations alongside the automated verdict.

### 5. Package Independence

During implementation verification, pack both libraries locally:

```powershell
dotnet pack src/Monstertacle.SharpQrs/Monstertacle.SharpQrs.csproj --configuration Release --no-build --output artifacts/packages
dotnet pack src/Monstertacle.SharpQrs.AspNetCore/Monstertacle.SharpQrs.AspNetCore.csproj --configuration Release --no-build --output artifacts/packages
```

The full verification entry point already performs this check through
`scripts/check-packages.ps1`; the pack commands above are for individual inspection.
Create temporary consumers outside the source tree using that local feed plus
nuget.org. Run both usage modes with package references only and no SharpClaw access.
Inspect MIT package metadata and retained notices. Do not push packages to any feed.

## Evidence and Limitations

Successful scripts prove only the checks they execute. Record explicit review of
SOLID, British English, public API contracts, third-party notices and advisory metrics.
An unavailable audit feed, missing tool, failed restore or incomplete measurement
must be reported; no result has passed yet merely because this guide exists.

A clean source snapshot provides interim portability evidence, not completion of
SC-004. Final acceptance requires this guide to pass from a fresh checkout of an
authorised Git commit. Git initialisation and committing require separate permission;
leave the criterion outstanding until that permission and verification exist.

Proceed to implementation only after the separate Tasks and Analyze phases and
their required approvals.
