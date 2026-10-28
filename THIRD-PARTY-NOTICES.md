# Third-party notices

The implementation is independent; no SharpClaw source is copied. The project's
MIT licence does not grant rights to unrelated source repositories.

The production integration references `Microsoft.Extensions.DependencyInjection.Abstractions`
10.0.14 (MIT). Its original licence and notices remain with the NuGet distribution.
The dependency-free core has no third-party package dependency.

Development-only dependencies are not bundled into the production libraries:

| Component | Version | Licence |
| --- | --- | --- |
| Microsoft.Extensions.DependencyInjection | 10.0.14 | MIT |
| Microsoft.AspNetCore.TestHost | 10.0.14 | MIT |
| Microsoft.NET.Test.Sdk and test platform dependencies | 18.11.0 | MIT |
| xUnit framework and Visual Studio adapter | 2.9.3 / 4.0.1 | Apache-2.0 |
| Coverlet MSBuild | 10.1.0 | MIT |
| Codemetr.CSharp.Tool | 0.1.0 | MIT |

The published metrics tool's dependency manifest additionally identifies Roslyn
`Microsoft.CodeAnalysis.Common` and `Microsoft.CodeAnalysis.CSharp` 5.3.0 (MIT),
and `Microsoft.Extensions.FileSystemGlobbing` 10.0.0 (MIT). These remain separate
local tool contents, not redistributed library contents. The application package
audit covers the solution's direct/transitive packages, not all SDK/tool binaries.
Consult each original package for its complete third-party notices.
