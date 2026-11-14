# Research: dual-target support

## Decisions

- Use stable SDK 11.0.100, advertised by the official .NET 11 download page and
  release metadata at https://builds.dotnet.microsoft.com/dotnet/release-metadata/11.0/releases.json.
  Keep .NET 10 installed for pinned tools and legacy runtime checks. Prefer isolated
  user installation to changing system SDK defaults.
- Multi-target net10.0/net11.0 and condition central Microsoft package versions.
  Retain existing MIT Microsoft dependency families and existing test/tool versions;
  audit actual resolved versions rather than assuming their security status.
- Keep one root release manifest and feed; separate each target's test and coverage
  evidence. The plan research agent independently inspected the existing scripts
  and confirmed release selection requires a single manifest. Root checked those
  scripts too; its conclusions inform this design rather than substitute for tests.
- Preserve .NET 10 source/binary compatibility with the published baseline; validate
  new target assets with separate package consumers. Do not retarget the baseline.

## Skill sources read

`dotnet/skills` revision `2877c712edc20643b72f64e2983b168e6661d607`:
`plugins/dotnet-upgrade/skills/migrate-dotnet10-to-dotnet11/SKILL.md`, its
`references/{csharp-compiler,core-libraries,sdk-msbuild,aspnetcore}-dotnet10to11.md`,
and `plugins/dotnet-test/skills/platform-detection/SKILL.md`.
The migration guide covers previews and is not exhaustive for stable 11.0;
its framework, build, dependency and behavioural checks apply, but its preview
version examples are not adopted. Existing source does not use EF, OpenAPI,
cryptography or compression APIs requiring suggested source migrations.

`codemetr/skills` revision `32e403e131b988a9058cab4045869a7a99ae858e`:
`plugins/codemetr-csharp/skills/guard/SKILL.md` and
`plugins/codemetr-csharp/references/tool.md`.
Installed `codemetr-csharp-cover` 0.1.1: `skills/coverage-guard/SKILL.md` and
`references/tool.md`. Reuse pinned .NET 10 tools with reports for each target;
no claim that those tools target .NET 11 themselves.

## Alternatives

Single-target replacement loses approved legacy support. Rolling previews reduce
reproducibility. Aggregated coverage hides target regressions. Separate full runs
complicate existing release artifact selection. None is needed.
