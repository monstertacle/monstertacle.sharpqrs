param([Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
function Invoke-PackageCommand {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Package-consumer command failed: dotnet $Arguments" }
}
$root = (Get-Location).Path
$feed = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) 'packages'
New-Item $feed -ItemType Directory -Force | Out-Null
foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
    Invoke-PackageCommand @('pack', "src/$name/$name.csproj", '-c', 'Release', '--no-build', '--no-restore', '--output', $feed)
    $package = Join-Path $feed "$name.0.1.0-alpha.1.nupkg"
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $nuspec = @($archive.Entries | Where-Object FullName -Like '*.nuspec')
        if ($nuspec.Count -ne 1) { throw 'Missing package metadata.' }
        $reader = [IO.StreamReader]::new($nuspec[0].Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($metadata.package.metadata.license.'#text' -ne 'MIT' -or $metadata.package.metadata.version -ne '0.1.0-alpha.1') { throw 'Incorrect package licence or version.' }
        if ($name -eq 'Monstertacle.SharpQrs' -and $metadata.package.metadata.dependencies.group.dependency) { throw 'The core package must be dependency-free.' }
        if (-not ($archive.Entries | Where-Object FullName -eq 'LICENSE')) { throw 'The MIT notice is missing from the package.' }
    } finally { $archive.Dispose() }
}
$temporaryBase = Join-Path $HOME 'dev/.agenttemp'
New-Item $temporaryBase -ItemType Directory -Force | Out-Null
$scratch = Join-Path $temporaryBase "sharpqrs-consumers-$([guid]::NewGuid().ToString('N'))"
New-Item $scratch -ItemType Directory | Out-Null
try {
    Copy-Item global.json (Join-Path $scratch 'global.json')
    foreach ($mode in @('standalone', 'integrated')) {
        $directory = Join-Path $scratch $mode
        New-Item $directory -ItemType Directory | Out-Null
        Copy-Item examples/Standalone/*.cs $directory
        $references = '<PackageReference Include="Monstertacle.SharpQrs" Version="0.1.0-alpha.1" />'
        if ($mode -eq 'integrated') {
            $references += '<PackageReference Include="Monstertacle.SharpQrs.AspNetCore" Version="0.1.0-alpha.1" /><PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.14" />'
            $program = @'
using Microsoft.Extensions.DependencyInjection;
using Monstertacle.SharpQrs;
using Monstertacle.SharpQrs.AspNetCore;
namespace Monstertacle.SharpQrs.Examples.Standalone;
internal static class Program
{
    private static async Task Main()
    {
        var services = new ServiceCollection();
        services.AddSharpQrsCommand<EchoCommand, EchoResult, EchoCommandHandler>();
        services.AddSharpQrsQuery<EchoQuery, EchoResult, EchoQueryHandler>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using var scope = provider.CreateAsyncScope();
        var command = await scope.ServiceProvider.GetRequiredService<ICommandDispatcher>().DispatchAsync(new EchoCommand("package"));
        var query = await scope.ServiceProvider.GetRequiredService<IQueryDispatcher>().DispatchAsync(new EchoQuery("package"));
        if (command.Value != "package" || query.Value != "package") throw new InvalidOperationException("Packaged dispatch failed.");
        Console.WriteLine("Integrated package consumer passed.");
    }
}
'@
            Set-Content (Join-Path $directory 'Program.cs') $program
        }
        $project = "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup><ItemGroup>$references</ItemGroup></Project>"
        $projectPath = Join-Path $directory 'Consumer.csproj'
        Set-Content $projectPath $project
        Invoke-PackageCommand @('restore', $projectPath, '--source', $feed, '--source', 'https://api.nuget.org/v3/index.json', '--packages', (Join-Path $scratch 'packages'))
        Invoke-PackageCommand @('run', '--project', $projectPath, '-c', 'Release', '--no-restore')
        $runtime = Get-Content (Join-Path $directory 'bin/Release/net10.0/Consumer.runtimeconfig.json') -Raw
        if ($mode -eq 'standalone' -and $runtime.Contains('Microsoft.AspNetCore.App')) { throw 'Standalone consumer requires ASP.NET Core.' }
    }
} finally {
    # Only this invocation's unique directory is owned by this script.
    Remove-Item -LiteralPath $scratch -Recurse -Force
}
