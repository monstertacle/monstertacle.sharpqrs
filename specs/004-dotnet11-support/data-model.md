# Verification evidence model

No domain data model changes.

- Root run: revision, SDK, PowerShell, packageVersion, coverageBase, workingTree,
  startedUtc, finishedUtc, status and frameworks (exactly net10.0 and net11.0).
  Only after all stages pass may status transition from running to passed.
- Framework evidence: tests.trx, unique collector report, coverage.cobertura.xml
  and cover.json inside the framework directory. Each report belongs to one target.
- Package pair: core and integration with matching version; each contains both
  target assemblies. Core has no package dependencies; integration uses matching DI.
- Audit inventory: one solution project entry and one entry for each supported target;
  every non-project lock dependency must equal a listed dependency and version.
