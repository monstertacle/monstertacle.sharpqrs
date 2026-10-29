# Research and decisions

## Explicit execution modes

Decision: additive synchronous contracts. Existing asynchronous contracts are public
and already published. Adding mandatory members would break implementers. Blocking
an async dispatcher would violate the Constitution. Automatic adaptation would obscure
mode selection. Separate contracts permit narrow consumer dependencies.

## Interface service composition

Decision: select exactly one unkeyed handler service descriptor, validate without
activation at mapping and dispatcher construction, resolve lazily in the current scope.
Alternative: automatic scanning adds ambiguity and reflection. Registering interface
aliases can duplicate disposal. Neither is required. Existing concrete helpers remain
an appropriate composition-root convenience, not a domain dependency.

## Coverage

Decision: reuse VSTest collector reports and TRX; retain independent library gates.
A marketplace skill is guidance, while a pinned tool invocation and strict report
validation provide enforcement. Tool defaults and the explicit initial profile are
recorded in plan.md. No baseline or library exclusions.

## Skill sources

Spec Kit 1.2.2 repository skills: clarify, plan, tasks, analyze, implement. No extension
hooks configured. Dotnet skills source e87c5da26cbf0f8c701e91247c4817df0adc12f7:
plugins/dotnet-test/skills/run-tests and platform-detection. The old csharp-expert
path is absent from that newer checkout; inspected csharp-refactoring and classified
this work as an additive feature under the repository feature workflow, not a pure
behaviour-preserving refactor. Existing API behaviour remains under regression tests.
Codemetr guard/reference read from the established local skills checkout; coverage
plugin 0.1.0 and provenance are recorded in coverage-source-review.md. These sources
are .NET 10 compatible; no host-specific hooks or alternative test runner is adopted.
