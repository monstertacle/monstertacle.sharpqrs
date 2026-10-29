# Implementation plan for corrected DI composition

**Created**: 2026-10-29
**Status**: done
**Owner**: repository maintainer, Alexander Bleissem
**Authority**: user requested autonomous implementation; checked construction explicitly approved in the latest user instruction.

## Technical context and Constitution check

Use existing C#14/net10.0, SDK 10.0.403, xUnit2/VSTest/Coverlet, PowerShell7,
Codemetr 0.1.0 and coverage tool 0.1.1. No new runtime package, tool version,
baseline, exclusion or numerical rule. Preserve dependency-free core, DI abstractions
integration, British English, explicit ownership and non-blocking async execution.
Constitution 2.0.0 reviewed before design. The user authorised phase progression;
the separate breaking setup choice remains an explicit prerequisite for its tasks.

## Independent mode-isolation slice

Extract common concrete-handler type/ownership validation into its own internal
responsibility. Async helpers validate async infrastructure then that shared rule;
sync helpers validate sync infrastructure then that same shared rule. Ensure only
unkeyed concrete descriptors participate. Keep owned descriptor identity for later
complete validation. Execute failing sync command/query tests alongside an external
async dispatcher before implementation; preserve real same-mode conflict failures
and cross-mode concrete sharing. This slice does not depend on a new public API.

## Approved checked composition slice

Implement the additive PrepareSharpQrs snapshot and SharpQrsServiceProviderFactory
wrapper described in contracts/composition.md following owner approval of the
required construction change for unreleased interface helpers. Represent contract
selection as immutable type metadata, not closures capturing IServiceCollection.
Preparation validates actual descriptors and owned concrete identity, removes stale
prepared metadata from copied collections, creates a separate read-only collection
and attaches fresh immutable selected-service evidence. No handler activation,
provider construction, reflection or mutation of the input.

The factory wraps IServiceProviderFactory<IServiceCollection> supplied by its host.
CreateBuilder delegates unchanged; CreateServiceProvider prepares the final builder
then calls the inner provider factory. No new DI implementation dependency. Host
container configuration therefore finishes before validation. Existing published
concrete-only paths can still use the ordinary builder; complete guarantees require
checked preparation. Unprepared interface mapping resolution fails with migration
instructions instead of inspecting an unrelated collection.

## Tests and examples

Execute copied collection zero/one/two selection tests for all four selectors,
including original edits, two independent providers, keyed services, no handler
activation, failure atomicity and read-only snapshots. Test before/after concrete
conflicts and removed/replaced owned descriptors. Preserve published consumers.
Update existing checked integration fixtures for the approved earlier failure
boundary rather than deleting their negative assertions. Keep explicit legacy
compatibility coverage for published plain provider construction.

Add a Generic Host worker demonstration inside the existing Web example project
(no new solution project or runtime dependency): async scope per unit of work,
constructor-injected scoped application work, command/query scope sharing, distinct
units and async disposal. Add a decorated handler example with explicit container
ownership. Include their executed source in coverage and existing package checks.

## Quality and completion

All requirements map to tasks.md. Full Release verification and complete cover/static
checks, audit, runnable examples, package consumers and binary/source compatibility
must pass; review untested branches, SOLID and dependency direction. Resolve each
origin finding in the follow-up plan. Commit/push and complete checked PR autonomously.
No package publication is part of this task.
