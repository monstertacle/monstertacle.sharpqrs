$ErrorActionPreference = 'Stop'
function Invoke-VerificationStages {
    param([scriptblock]$Runner)
    foreach ($stage in @('restore', 'build', 'tests', 'coverage', 'metrics', 'audit', 'standalone', 'web', 'packages')) {
        $result = & $Runner $stage
        if ($result -ne 0) { throw "Verification stage $stage failed with exit $result." }
    }
}
function Assert-VerificationScripts {
    param([string]$Root)
    foreach ($name in @('coverage', 'metrics', 'audit', 'packages', 'web')) {
        if (-not (Test-Path -LiteralPath (Join-Path $Root "scripts/check-$name.ps1"))) { throw "Missing mandatory $name checker." }
    }
}
function Invoke-CheckedCommand {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments | ForEach-Object { Write-Host $_ }
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed with exit $LASTEXITCODE; arguments: $Arguments" }
}
function Publish-CoverageReport {
    param([string]$Directory)
    $destination = Join-Path $Directory 'coverage.cobertura.xml'
    if (Test-Path -LiteralPath $destination) { throw 'Coverage output must be fresh.' }
    # Collector output is directly inside a GUID directory. TRX's machine-specific
    # attachment mirrors are copies of this report, not additional collection runs.
    $reports = @(Get-ChildItem -LiteralPath $Directory -Directory | Where-Object {
        $identifier = [guid]::Empty
        [guid]::TryParse($_.Name, [ref]$identifier)
    } | ForEach-Object { Get-ChildItem -LiteralPath $_.FullName -File -Filter 'coverage.cobertura.xml' })
    if ($reports.Count -ne 1) { throw "Expected exactly one collector report, found $($reports.Count)." }
    Copy-Item -LiteralPath $reports[0].FullName -Destination $destination
    return $destination
}
function Invoke-RepositoryVerification {
    $root = Split-Path $PSScriptRoot -Parent
    Push-Location $root
    try {
        Assert-VerificationScripts $root
        foreach ($name in @('CheckCoverage', 'CheckMetrics', 'CheckAudit', 'CheckPackages', 'Verify')) {
            Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', "tests/Scripts/$name.Tests.ps1")
        }
        $started = [datetime]::UtcNow
        $run = Join-Path $root "artifacts/verify-$($started.ToString('yyyyMMddTHHmmssZ'))-$([guid]::NewGuid().ToString('N'))"
        New-Item $run -ItemType Directory | Out-Null
        $revision = & git rev-parse HEAD
        if ($LASTEXITCODE -ne 0) { throw 'Unable to identify source revision.' }
        $manifest = [ordered]@{ revision = $revision; startedUtc = $started.ToString('o'); sdk = (& dotnet --version)
            powershell = $PSVersionTable.PSVersion.ToString(); status = 'running'; workingTree = (& git status --porcelain) }
        $manifest | ConvertTo-Json -Depth 5 | Set-Content "$run/run.json"
        Invoke-VerificationStages {
            param($stage)
            Write-Host "Verifying $stage..."
            switch ($stage) {
                'restore' {
                    Invoke-CheckedCommand 'dotnet' @('tool', 'restore')
                    Invoke-CheckedCommand 'dotnet' @('restore', 'Monstertacle.SharpQrs.slnx', '--locked-mode')
                }
                'build' { Invoke-CheckedCommand 'dotnet' @('build', 'Monstertacle.SharpQrs.slnx', '-c', 'Release', '--no-restore') }
                'tests' {
                    Invoke-CheckedCommand 'dotnet' @('test', 'Monstertacle.SharpQrs.slnx', '-c', 'Release', '--no-build', '--no-restore',
                        '--results-directory', $run, '--logger', 'trx;LogFileName=tests.trx',
                        '--collect', 'XPlat Code Coverage', '--settings', 'coverage.runsettings')
                    Publish-CoverageReport $run | Out-Null
                    [xml]$trx = Get-Content "$run/tests.trx" -Raw
                    if ([int]$trx.TestRun.ResultSummary.Counters.executed -le 0 -or [int]$trx.TestRun.ResultSummary.Counters.failed -ne 0) { throw 'No passing executed tests.' }
                }
                'coverage' { Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', 'scripts/check-coverage.ps1', '-ReportPath', "$run/coverage.cobertura.xml", '-NotBefore', $started.ToString('o')) }
                'metrics' { Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', 'scripts/check-metrics.ps1', '-ReportPath', "$run/metrics.json") }
                'audit' { Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', 'scripts/check-audit.ps1', '-OutputDirectory', $run) }
                'standalone' { Invoke-CheckedCommand 'dotnet' @('run', '--project', 'examples/Standalone', '-c', 'Release', '--no-build', '--no-restore') }
                'web' { Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', 'scripts/check-web.ps1', '-OutputDirectory', $run) }
                'packages' { Invoke-CheckedCommand 'pwsh' @('-NoProfile', '-File', 'scripts/check-packages.ps1', '-OutputDirectory', $run) }
            }
            return 0
        }
        $manifest.status = 'passed'
        $manifest['finishedUtc'] = [datetime]::UtcNow.ToString('o')
        $manifest | ConvertTo-Json -Depth 5 | Set-Content "$run/run.json"
        Write-Host "All automated checks passed. Reports: $run"
        Write-Host 'SOLID, compatibility, language, licences and advisory metrics still require documented review.'
    } finally { Pop-Location }
}
if ($MyInvocation.InvocationName -ne '.') { Invoke-RepositoryVerification }
