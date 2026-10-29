# Coverage integration source review

The user requested permanent coverage integration through the marketplace as an
extension to the feature specification. This records completed setup separately
from proposed repository implementation.

## Completed marketplace setup

- Added marketplace `codemetr/skills` using the host's supported marketplace command.
- Installed `codemetr-csharp-cover@codemetr-skills`, plugin version **0.1.0**.
- Read and applied `skills/coverage/SKILL.md`,
  `skills/coverage-guard/SKILL.md` and `references/tool.md` in that plugin.
- Marketplace source revision: `2cf9314d80e847aad82a18574266703a1c455e9c`.
- Source: [marketplace plugin](https://github.com/codemetr/skills/tree/2cf9314d80e847aad82a18574266703a1c455e9c/plugins/codemetr-csharp-cover).

Installation was confirmed by the host command returning the plugin identifier,
version and installed path. It changes the local assistant environment; it does
not install the coverage executable or enforce a repository check.

## Findings for planning

- The separate executable package is `Codemetr.CSharp.Cover.Tool`; its command is
  `codemetr-csharp-cover`. It requires .NET 10, compatible with this repository.
- Direct verification of the public NuGet V3 index confirms published executable
  versions **0.1.0** and **0.1.1**. The planned executable version is **0.1.1**;
  the marketplace plugin remains **0.1.0**. Plugin setup examples targeting tool
  0.1.0 do not override the selected executable version. Planning must pin 0.1.1
  in the existing local tool manifest and verify its command behaviour.
  Source: [published tool](https://www.nuget.org/packages/Codemetr.CSharp.Cover.Tool/0.1.1),
  checked against `https://api.nuget.org/v3-flatcontainer/codemetr.csharp.cover.tool/index.json`.
- Existing tests already use VSTest and Coverlet collector. No test-platform switch
  is required by the documented integration.
- Existing verification creates Cobertura and TRX files. The tool supports reading
  these together; complete check mode must reject untrustworthy or partial evidence.
- The current CI checkout is shallow. Diff checks need an explicit valid comparison
  base and sufficient history, including a defined policy for main and release runs.
- Examples are part of the solution. Planning must establish complete production
  measurement scope without pretending that excluded handwritten code was measured.
- The existing per-library 90% checker remains mandatory. The coverage tool's
  aggregate rules cannot replace this independent gate.
- Tool defaults include diff branch and risk limits. They are not automatically
  approved project thresholds. Planning must classify them explicitly.
- No baseline has been created; no tool binary has been installed; no coverage
  result is claimed. Requirements FR-011–FR-014 and acceptance SC-006–SC-007 track
  the remaining configuration, tests and pipeline work within this feature.

Tool documentation reviewed at revision
`eb78a48866090be49eba7985a1224cda537b970b`:
[upstream README](https://github.com/codemetr/Codemetr.CSharp.Cover/blob/eb78a48866090be49eba7985a1224cda537b970b/README.md).
