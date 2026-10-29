# Implementation Plan: Interface Composition and Synchronous CQRS

**Branch**: `002-interface-sync-cqrs`
**Date**: 2026-10-29
**Status**: done
**Owner**: repository maintainer, executed in the current working session
**Authority**: User requested autonomous Spec Kit implementation after reviewing scope.

## Summary

Add explicit synchronous contracts without changing existing asynchronous interfaces.
Expose separate standalone registration contracts. Add interface-service mapping
helpers to the DI boundary, retaining existing concrete registration and ownership.
Pin Codemetr.CSharp.Cover.Tool 0.1.1 and enforce complete coverage checks alongside
the existing independent per-library coverage gate.

## Technical Context

C# 14, .NET SDK 10.0.403, net10.0, PowerShell 7. Existing xUnit 2/VSTest/Coverlet
collector and package locks remain. No runtime dependencies are added. The new MIT
coverage development tool is restored from nuget.org in the existing tool manifest.
Its published package, command help and doctor output are checked before use.

## Constitution Check

Constitution 2.0.0 checked before and after design. Core remains dependency-free;
DI resolution remains in integration adapters. TDD, nullable warnings-as-errors,
per-type source files, existing strict metrics, ownership and cancellation contracts
remain mandatory. British English applies. User's explicit autonomous instruction
supplies phase progression authority. No breaking interface change or publication.
No baseline is created. No existing numerical rule is weakened.

## Project Structure

Extend existing src projects; new files contain one declared type each. Add tests
under Core and Integration and shared test fixtures. Coverage orchestration lives in
scripts/check-cover.ps1 with dependency-free negative fixtures in tests/Scripts.
Use the existing verifier and reusable Windows/Linux workflow, including release reuse.

## Design

- ICommandRegistration and IQueryRegistration expose the existing Register signatures;
  CommandDispatcher and QueryDispatcher implement them without changing existing APIs.
- ISyncCommandHandler and ISyncQueryHandler expose Handle returning TResult and taking
  the original cancellation token. ISyncCommandDispatcher and ISyncQueryDispatcher
  expose Dispatch. Separate ISyncCommandRegistration and ISyncQueryRegistration
  provide setup-only access. SyncCommandDispatcher and SyncQueryDispatcher compose a
  synchronous registry; they do not block on tasks or adapt execution modes.
- Sync routing duplicates only mode-specific invocation, reusing the existing immutable
  HandlerRegistration record. Exact request/result keys, null rejection, duplicate
  preservation and original exception identity match the async contract.
- AddSharpQrsCommand<TRequest,TResult>() and AddSharpQrsQuery<TRequest,TResult>() map
  existing handler-interface services. Existing three-argument helpers remain intact.
- AddSharpQrsSynchronous registers the two scoped synchronous dispatchers idempotently.
  AddSharpQrsSyncCommand/Query offer contract-based two-argument and concrete-handler
  three-argument helpers. A separate extension type keeps each coherent surface bounded.
- Contract mapping requires exactly one unkeyed descriptor for its handler contract
  at setup and checks that selection again when a scoped dispatcher is constructed.
  No handler is activated for validation. Additional registrations before provider
  construction cannot silently make a previously selected service ambiguous.
  The service collection must not change after provider construction.
- Contract adapters resolve the selected service lazily. They never implement disposal
  or introduce disposable aliases; service owners control supplied lifetimes.
  Concrete synchronous registrations reuse the existing owned concrete-type marker
  and scoped lifetime, including cross-mode sharing of one concrete handler.
- Rejected mappings validate infrastructure, descriptor selection and duplicates
  before any collection mutation. Sync infrastructure has independent owned metadata.

## Coverage policy

Pin tool 0.1.1; run doctor and reuse current fresh Cobertura plus TRX output.
Require schema/tool/version, check command, passed=true, complete=true, no parse or
input errors, matching HEAD, nonempty measured production inventory and no unreported
production members. Preserve process exits and fresh outputs. Compare against explicit
base: PR target SHA; pushes use first parent; release/manual runs use first parent;
local defaults to origin/main, with sufficient fetched history and failure if unresolved.

Initial coverage policy: aggregate line coverage >=90%, diff line coverage >=90%,
CRAP <=30 (documented tool default, advisory complexity still reviewed). Whole-run,
per-type, per-member and diff branch floors remain 0: branch measurements are reported
and reviewed, with no separately approved numerical branch floor. Per-type/member
line floors remain 0 because the agreed floor is per-library, enforced independently.
Unreported handwritten code counts as zero. No baseline or source exclusions.
Both examples are referenced by tests and included in collector instrumentation;
standalone execution and actual loopback HTTP requests exercise them in-process.
The independent library gate accepts only the two libraries and these two known
example assemblies, and still evaluates each library separately. Initial exclusion
of examples was rejected by the tool's unmeasured-change guard; scope was expanded.

The user authorised integration and autonomous design; this explicit initial coverage
profile is within that scope. Existing static metrics and per-library 90% floors remain.

## Verification and compatibility

Execute meaningful failing tests against compilable stubs before implementation.
Retain all existing tests. Check all eight composition/request/mode combinations,
wrong-mode isolation, token and exception identity, nulls, duplicates, lazy activation,
missing/ambiguous selections, decorators, concurrent calls and disposal.
Compile a consumer against published 0.1.0-alpha.1 and run it with candidate assemblies;
also compare the old public interface signatures and run unchanged consumer source.
Full verifier: Release build, tests, coverage, complete static guard, audit, examples,
local packages and isolated consumers. Commit/push, open PR, await both OS jobs and
merge only the checked head. Retain evidence and resolve every actionable finding.

## Complexity Tracking

New contracts separate execution modes and setup needs; no interface for every class.
A synchronous registry is separate because its invocation and error timing are direct.
Container-specific adapters remain outside core. No retries, scanning or global state.


## Acceptance and delivery

All implementation tasks and local gates passed. The committed candidate
`309a60bfbce80d783c8841a2803e15c3065c2ca6` passed branch and PR verification on
Windows and Linux. PR merge candidate `a5f4032d25a021e91f406e1f758baf1b6ce45bc8`
was verified from clean checkouts, including both library line floors, full coverage,
compatibility, static metrics, audit, examples and package consumers. PR #3 was
mergeable against unchanged target `b1535a0d265230201dea0aed2cd7c52d1bf13f11`.

This merge includes the final delivery record and completes
[PR #3](https://github.com/monstertacle/monstertacle.sharpqrs/pull/3) on main.
The feature branch is removed after integration. No new NuGet version is published.
Evidence and finding dispositions are in implementation-evidence.md; no actionable
acceptance finding remains open. Source code is identical to the checked PR head;
only this delivery documentation is added during merge.
