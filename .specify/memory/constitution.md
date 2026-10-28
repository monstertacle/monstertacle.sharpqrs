# Monstertacle.SharpQrs Constitution

**Status**: Ratified by the user on 2026-10-28.

## Core Principles

### I. Independent Libraries and Bounded Scope

- The CQRS core MUST be usable without a web host or an ASP.NET Core service
  container. ASP.NET Core registration MUST reside in a separate library which
  references the core, never the reverse.
- Both libraries MUST build and operate without SharpClaw sources.
  SharpClaw is a reference implementation, not a build or runtime dependency.
- Every additional feature and abstraction MUST have a specified requirement.
  Hypothetical extensibility does not justify speculative abstraction layers.

These boundaries preserve independent use and prevent unnecessary coupling.

### II. SOLID, Explicit Contracts and Idiomatic C#

- Classes MUST have a cohesive responsibility (SRP). Suitable contracts and
  composition MUST support extension (OCP), without requiring an interface for
  every class merely as a precaution.
- Implementations MUST honour their abstractions' behavioural contracts (LSP).
  Interfaces MUST be limited to consumer needs (ISP). Domain execution MUST remain
  independent of concrete infrastructure (DIP).
- Container resolution MUST be confined to the integration boundary;
  service-locator access within domain handlers is prohibited.
- Public contracts MUST define results, errors, nullability, cancellation semantics,
  thread safety, registration timing and lifetimes. Resource ownership and disposal
  responsibilities MUST be explicit.
- Hidden global mutable state is prohibited. Concurrency guarantees MUST be tested;
  handler thread safety MUST NOT be assumed or implied without evidence.
- Nullable reference types MUST be enabled. Compiler warnings and warnings from
  the explicitly selected analyser profile MUST be treated as errors.
- Blocking waits on tasks, unobserved task failures and silently swallowed errors
  are prohibited. Cancellation and disposal MUST be handled explicitly.
- Each hand-written declared C# type MUST have its own file. Artificial splitting
  and cryptic shorthand solely to satisfy metrics are prohibited.

SOLID compliance and readability MUST be reviewed; metrics do not prove either.

### III. Test-driven Development and Effective Tests

- New behaviour and defect corrections MUST follow Red -> Green -> Refactor.
  Before implementing behaviour, an executed test MUST fail because that behaviour
  is missing. Minimal compilable types and signatures may be introduced beforehand;
  a compilation failure alone is not sufficient evidence of Red.
- Implementation MUST initially be limited to the behaviour required to pass the
  test. Refactoring MUST preserve passing tests.
- Documentation-only, build and configuration changes do not require artificial
  behavioural tests, but MUST receive verification appropriate to the change.
- Tests MUST verify public contracts using meaningful assertions. Relevant error,
  cancellation, concurrency and disposal scenarios MUST be covered. Integration
  tests MUST use real service registration and resolution, including scopes.
- Tests MUST run independently and deterministically. Implementation evidence MUST
  make Red and Green outcomes traceable without requiring permanent raw logs.
- Each production library MUST achieve at least 90% line coverage. An aggregate
  average MUST NOT conceal a library falling below this threshold. Branch coverage
  MUST also be reported and reviewed for relevant untested branches; a numerical
  branch threshold requires separate approval.
- Coverage MUST come from tests actually executed against the source revision under
  review. Test code and genuinely generated code are outside the denominator;
  hand-written production code MUST NOT be excluded to improve results.
  Empty or incomplete measurements MUST NOT be reported as passing.

Coverage is a minimum standard, not a substitute for TDD or evidence of effective tests.

### IV. Strict, Traceable Code Metrics

- Codemetr.CSharp MUST be used as a mandatory guard. Its version, configuration and
  invocation MUST be version-controlled and reproducible.
- All hand-written C# code, including tests and examples, MUST be measured.
  Only genuinely generated code and build artefacts may be excluded.
- Strict code metrics, including type size, MUST be enforced. Limits apply to the
  complete logical type; partial declarations MUST NOT circumvent them.
  Compliance across partial declarations MUST be checked in aggregate.
- Numerical guard thresholds MUST be justified and approved during planning using
  representative contract, dispatcher, integration and test types. Configurable
  numerical rules MUST be stricter than the documented defaults of the selected
  tool version. Existing qualitative rules MUST NOT be weakened.
  Implementation MUST NOT begin without an approved metrics profile.
- Each available metric MUST be classified as an automated gate, a mandatory review
  finding or not applicable with justification. Advisory metrics MUST NOT be
  represented as automatically passing. Where available, these include cognitive
  complexity, cohesion and directed dependencies.
- Detected dependency cycles MUST be reviewed in context. Unintended architectural
  cycles and any dependency from the core back to the integration block acceptance.
- Guard errors, threshold violations and incomplete measurement scope block
  acceptance. Baselines admitting new violations, blanket suppressions and relaxed
  overrides solely to obtain a passing result are prohibited.
- Analysis limitations MUST be disclosed. Missing optional external measurements,
  such as mutation reports, MUST be labelled as not collected, not as passing.
  Agreed mandatory measurements MUST NOT be reclassified as optional.

### V. Specification First and Staged Approval

- Work MUST follow Constitution -> Specify -> Clarify -> Plan -> Tasks -> Analyze ->
  Implement, with TDD and final verification. These names identify Spec Kit commands.
- Each phase MUST be completed and the user MUST approve the transition before the
  next phase begins. Clarify MUST resolve questions or explicitly record that none
  remain. Analyze MUST check requirements, design, tasks and Constitution for consistency.
- Requirements MUST have stable identifiers and verifiable acceptance criteria.
  Plans and tasks MUST be traceable to them.
- Significant architectural decisions MUST record their rationale and alternatives.
  Contradictions and premature work MUST be reviewed and corrected before reuse;
  such work is not automatically approved.

### VI. British English Throughout

- All project-authored natural-language content and project communication MUST use
  British English consistently. This includes specifications, plans, documentation,
  comments, XML documentation, test descriptions, diagnostics, commit messages,
  pull requests, release notes and assistant responses.
- Project-defined identifiers containing natural-language words MUST use British
  English spelling where a regional distinction exists.
- External API identifiers, command names, paths, URLs, proper names and verbatim
  third-party material MUST retain their required or original spelling. This
  preserves technical correctness and legal notices; it does not permit original
  project prose in another language.
- Existing non-English project prose MUST be translated when brought into the
  approved workflow and before its acceptance. Language compliance MUST be reviewed.

## Technical Constraints and Sources

- The programming language is C#; libraries and tests MUST target .NET 10.
  ASP.NET Core integration MUST use major version 10. SDK and development tools
  MUST be selected reproducibly. Preview features require explicit user approval.
- `https://github.com/dotnet/skills` MUST be used as a source of applicable development
  skills; `https://github.com/codemetr/skills` MUST be used for metrics analysis and
  guard skills. Relevant skills MUST be read and applied before the corresponding
  work. Sources MUST be traceable by path and commit or version.
- Skills MUST be checked for .NET 10 compatibility. They replace neither tool
  execution nor quality gates and MUST NOT weaken project rules. Missing skills or
  tools MUST be disclosed. Installation and version changes require user approval.
  Instructions intended for another host MUST NOT be followed blindly.
- Additional package dependencies MUST document their concrete benefit, licence
  compatibility, maintainability and known security risks. Restore MUST be
  reproducible. Unresolved relevant security findings block publication;
  verification limitations MUST be disclosed.
- Public API changes MUST be checked for source, binary and behavioural
  compatibility. Releases MUST use semantic versioning. Breaking changes require
  explicit approval and documented migration guidance.
- The project uses the MIT licence. The root `LICENSE` file MUST contain the
  unaltered MIT licence text with `Copyright (c) 2026 monstertacle`. Packages MUST
  declare MIT correctly; required third-party licence and attribution notices
  MUST be preserved.
- `.gitignore` MUST account for build artefacts, local IDE state, test and coverage
  reports, and local secret files. Specifications, skill references, quality
  configuration and lock files MUST remain eligible for version control.
  Ignore rules do not replace secret scanning.

## Development Workflow and Quality Gates

1. Before implementation, requirements, public contracts, test strategy, metrics
   profile and reproducible verification procedures MUST be planned and approved.
2. Tasks MUST schedule tests before behavioural implementation. Every requirement
   MUST have an appropriate acceptance check.
3. Before acceptance, the Release build, compiler and analyser checks, complete test
   suite, coverage gate and full Codemetr guard MUST pass. Mandatory review findings,
   including SOLID, public contracts and language compliance, MUST be resolved.
4. CI MUST enforce automated gates no later than the first merge or publication.
   Local and CI checks MUST use the same version-controlled rules.
   Publication is not automatically authorised.
5. Evidence MUST identify the checked source revision, tool versions, scope and
   results. Checks not performed and remaining limitations MUST be stated explicitly.
   Missing evidence MUST NOT be represented as success.

## Governance

This Constitution is binding following user approval on 2026-10-28.
Ratification does not automatically authorise transition to the next phase.

Once ratified, this Constitution governs feature artefacts and reviews. Amendments
require a documented rationale, an impact assessment and user approval. Configuration
may make rules concrete but MUST NOT weaken or circumvent them. Exceptions to minimum
standards require the same formal amendment procedure.

Versioning is semantic: MAJOR for incompatible rule changes, MINOR for new or materially
expanded rules, and PATCH for editorial clarification. Drafts carry a prerelease suffix.
Ratification removes that suffix and sets the ratification date. Later amendments
preserve that date and update the amendment date. Planning and acceptance reviews MUST
identify the Constitution version assessed.

**Version**: 2.0.0 | **Ratified**: 2026-10-28 | **Last Amended**: 2026-10-28
