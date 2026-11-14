# Implementation Plan: .NET 11 support

**Created**: 2026-11-14T08:33:04Z (measured with `date -u`)
**Status**: done (local implementation and acceptance)
**Owner**: current implementation session for Alexander Bleissem
**Authority**: user approved constitution amendment and all remaining phase transitions.

## Summary

Add net11.0 alongside net10.0 without changing the C# public API. Keep one complete
verification run, with per-framework evidence and two multi-target packages.

## Technical Context

C# 14, existing recommended analyser profile, SDK 11.0.100 plus SDK/runtime 10.0.403;
PowerShell 7; five existing projects. Central Microsoft dependencies retain 10.0.14
on net10.0 and use stable 11.0.0 on net11.0. Existing xUnit/VSTest/Coverlet and pinned
Codemetr tools remain unchanged. No new dependency kind or licence is introduced.

## Constitution Check

Constitution 2.1.0 is owner-approved. Preserve core independence, DI boundary,
contracts, strict metrics, nullable warnings-as-errors and British English. This is
configuration/build support; existing contract tests verify unchanged behaviour.
New non-trivial verification behaviour receives failing script regression checks
before implementation. No numerical limit, exclusion or baseline changes.
Owner approval covers autonomous phases and required SDK setup; publication remains
outside scope. The original .NET 10 binary compatibility check remains mandatory.

## Project Structure

Modify global.json, the five csproj files and Directory.Packages.props; regenerate
all five lock files. Adapt scripts/verify.ps1, check-audit.ps1, check-web.ps1,
check-packages.ps1 and release-packages.ps1 with their tests/Scripts regressions.
Update .github/workflows/verify.yml, README.md and docs/development.md.
Design artefacts live in specs/004-dotnet11-support/.

## Design and sequencing

1. Record baseline build. Install official SDKs in a dedicated user directory,
   without changing other projects' SDK resolution. Keep language/analyser rules.
2. Multi-target all existing projects; select matching central Microsoft versions.
3. Root verifier owns one run.json and packages/; net10.0 and net11.0 directories
   hold separate TRX, Cobertura and complete coverage-tool reports. Execute target
   tests/examples sequentially. Never merge coverage between targets.
4. Audit requires both frameworks exactly once per project, checks vulnerabilities
   at every level and reconciles every target against its lock section.
5. Package check requires both asset/dependency groups and four external consumers;
   shared package metadata and licence checks remain. Published compatibility uses
   net10.0 deliberately, preserving the actual original consumer binary.
6. Release selection requires the supported framework set in the successful root
   manifest. Preserve the existing two-OS matrix, artifact names and release flow.
7. Execute full verifier, review source diff, advisory metrics, language, licensing
   and compatibility, then record evidence. Do not claim remote CI until executed.

## Risk and alternatives

Replacing net10.0 would break support. Testing net11 only against a net10 package
would not provide a dedicated target or demonstrate matching integration behaviour.
Two complete verifier manifests would conflict with release selection; separate
framework reports inside one run preserve both evidence isolation and packaging.
The upstream migration skill is preview-era guidance; installed stable SDK and
executed checks determine actual compatibility. No speculative C# modernisation.

## Completion

All eleven tasks passed local acceptance; see implementation-evidence.md. Hosted
CI results and any subsequent integration are reported separately. No package was
published and no local acceptance finding remains open.
