param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
function Invoke-CompatibilityCommand {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Compatibility command failed: dotnet $Arguments" }
}
$root = (Get-Location).Path
$temporaryBase = Join-Path $HOME 'dev/.agenttemp'
New-Item $temporaryBase -ItemType Directory -Force | Out-Null
$scratch = Join-Path $temporaryBase "sharpqrs-compatibility-$([guid]::NewGuid().ToString('N'))"
New-Item $scratch -ItemType Directory | Out-Null
try {
    Copy-Item global.json $scratch
    @'
<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><auditSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></auditSources></configuration>
'@ | Set-Content (Join-Path $scratch 'NuGet.Config')
    @'
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup><PackageReference Include="Monstertacle.SharpQrs" Version="0.1.0-alpha.1" /><PackageReference Include="Monstertacle.SharpQrs.AspNetCore" Version="0.1.0-alpha.1" /><PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.14" /></ItemGroup></Project>
'@ | Set-Content (Join-Path $scratch 'Consumer.csproj')
    @'
using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs;
using Monstertacle.SharpQrs.AspNetCore;

var dispatcher = new CommandDispatcher();
dispatcher.Register(new OriginalHandler());
if (await dispatcher.DispatchAsync(new OriginalRequest()) != 42) throw new InvalidOperationException("Standalone compatibility failed.");
var services = new ServiceCollection();
services.AddSharpQrsCommand<OriginalRequest, int, OriginalHandler>();
services.AddSharpQrsQuery<OriginalRequest, int, OriginalHandler>();
await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
await using var scope = provider.CreateAsyncScope();
if (await scope.ServiceProvider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new OriginalRequest()) != 42)
    throw new InvalidOperationException("Scoped command compatibility failed.");
if (await scope.ServiceProvider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new OriginalRequest()) != 42)
    throw new InvalidOperationException("Scoped query compatibility failed.");
ICommandDispatcher thirdPartyCommand = new OriginalDispatcher();
IQueryDispatcher thirdPartyQuery = new OriginalDispatcher();
if (await thirdPartyCommand.DispatchAsync(new OriginalRequest()) != 42 || await thirdPartyQuery.DispatchAsync(new OriginalRequest()) != 42)
    throw new InvalidOperationException("Third-party interface implementation failed.");
Console.WriteLine("Published consumer binary and third-party implementations passed.");
'@ | Set-Content (Join-Path $scratch 'Program.cs')
    'using Monstertacle.SharpQrs; public sealed record OriginalRequest : ICommand<int>, IQuery<int>;' |
        Set-Content (Join-Path $scratch 'OriginalRequest.cs')
    @'
using Monstertacle.SharpQrs;
public sealed class OriginalHandler : ICommandHandler<OriginalRequest, int>, IQueryHandler<OriginalRequest, int>
{
    public Task<int> HandleAsync(OriginalRequest request, CancellationToken cancellationToken = default) => Task.FromResult(42);
}
'@ | Set-Content (Join-Path $scratch 'OriginalHandler.cs')
    @'
using Monstertacle.SharpQrs;
public sealed class OriginalDispatcher : ICommandDispatcher, IQueryDispatcher
{
    public Task<TResult> DispatchAsync<TResult>(ICommand<TResult> request, CancellationToken cancellationToken = default) => Task.FromResult((TResult)(object)42);
    public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> request, CancellationToken cancellationToken = default) => Task.FromResult((TResult)(object)42);
}
'@ | Set-Content (Join-Path $scratch 'OriginalDispatcher.cs')
    $project = Join-Path $scratch 'Consumer.csproj'
    Invoke-CompatibilityCommand @('restore', $project, '--configfile', (Join-Path $scratch 'NuGet.Config'), '--packages', (Join-Path $scratch 'packages'))
    Invoke-CompatibilityCommand @('build', $project, '-c', 'Release', '--no-restore')
    $binary = Join-Path $scratch 'bin/Release/net10.0/Consumer.dll'
    Invoke-CompatibilityCommand @($binary)
    foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
        Copy-Item (Join-Path $root "src/$name/bin/Release/net10.0/$name.dll") (Split-Path $binary -Parent) -Force
    }
    # Execute the original compiled binary, without recompiling against the candidate.
    Invoke-CompatibilityCommand @($binary)
    # Compile exactly the same consumer source against the candidate assemblies too.
    foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
        $published = Join-Path $scratch "packages/$($name.ToLowerInvariant())/0.1.0-alpha.1/lib/net10.0/$name.dll"
        Copy-Item (Join-Path $root "src/$name/bin/Release/net10.0/$name.dll") $published -Force
    }
    Invoke-CompatibilityCommand @('build', $project, '-c', 'Release', '--no-restore', '--no-incremental')
    Invoke-CompatibilityCommand @($binary)
    @{ baseline = '0.1.0-alpha.1'; sourceCompatibility = 'passed'; binaryCompatibility = 'passed';
        revision = (& git rev-parse HEAD) } | ConvertTo-Json | Set-Content (Join-Path $OutputDirectory 'compatibility.json')
} finally { Remove-Item -LiteralPath $scratch -Recurse -Force }
