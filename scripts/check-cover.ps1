param([string]$Directory, [string]$BaseRevision = 'origin/main', [datetime]$NotBefore = [datetime]::MinValue)
$ErrorActionPreference = 'Stop'
function Test-CoverReport {
    param([string]$Path, [string]$Revision, [string[]]$Inventory, [int]$ExitCode = 0, [datetime]$Started = [datetime]::MinValue)
    if ($ExitCode -ne 0) { throw "Coverage tool failed with exit $ExitCode." }
    if ((Get-Item -LiteralPath $Path).LastWriteTimeUtc -lt $Started.ToUniversalTime()) { throw 'Stale coverage tool report.' }
    $report = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
    if ($report.schema -ne 1 -or $report.tool -ne 'codemetr-csharp-cover' -or $report.version -ne '0.1.1' -or $report.command -ne 'check') {
        throw 'Unsupported coverage tool report.'
    }
    if ($report.passed -ne $true -or $report.complete -ne $true) { throw 'Coverage check failed or is incomplete.' }
    foreach ($key in @('parseErrors', 'inputErrors', 'violations', 'stale', 'unreported')) {
        if (-not $report.ContainsKey($key) -or $report[$key].Count -ne 0) { throw "Invalid coverage evidence: $key." }
    }
    $provenance = $report.provenance
    $head = [string]$provenance.head
    if ($head.Length -lt 7 -or -not $Revision.StartsWith($head, [StringComparison]::Ordinal)) { throw 'Coverage revision differs from source.' }
    if ($provenance.source -ne 'reports' -or $provenance.unreported -ne 'zero' -or $provenance.partial.Count -ne 0) { throw 'Incomplete coverage provenance.' }
    if ([string]::IsNullOrWhiteSpace($provenance.diff.base) -or [string]::IsNullOrWhiteSpace($provenance.diff.mergeBase) -or $provenance.diff.head -ne $head) {
        throw 'Missing or inconsistent coverage diff provenance.'
    }
    if ($provenance.reports.Count -eq 0 -or $provenance.testProjects.Count -eq 0) { throw 'Missing coverage or test evidence.' }
    foreach ($project in $provenance.testProjects) {
        if ($project.tests.total -le 0 -or $project.tests.failed -ne 0 -or $project.tests.passed -ne $project.tests.total -or $project.tests.skipped -ne 0) {
            throw 'Coverage requires complete passing, unskipped tests.'
        }
    }
    if ($report.summary.files -le 0 -or $report.summary.coverableLines -le 0) { throw 'Empty coverage measurement.' }
    foreach ($key in @('violations', 'unreported', 'unmapped')) {
        if (-not $report.summary.ContainsKey($key) -or $report.summary[$key] -ne 0) { throw "Coverage measurement has $key." }
    }
    $measured = @($report.types.file | ForEach-Object { $_.Replace('\', '/') } | Sort-Object -Unique)
    $expected = @($Inventory | ForEach-Object { $_.Replace('\', '/') } | Sort-Object -Unique)
    if ($expected.Count -eq 0 -or $report.summary.files -ne $expected.Count -or (Compare-Object $expected $measured)) { throw 'Coverage library/example inventory differs from source.' }
    return $true
}
function Invoke-CoverCheck {
    param([string]$RunDirectory, [string]$DiffBase, [datetime]$Started)
    $output = Join-Path $RunDirectory 'cover.json'
    if (Test-Path -LiteralPath $output) { throw 'Coverage output must be fresh.' }
    & dotnet tool run codemetr-csharp-cover -- check --reports (Join-Path $RunDirectory 'coverage.cobertura.xml') `
        --test-results (Join-Path $RunDirectory 'tests.trx') --base $DiffBase --output $output --format text
    $toolExit = $LASTEXITCODE
    $script:CoverageToolExitCode = $toolExit
    $revision = & git rev-parse HEAD
    if ($LASTEXITCODE -ne 0) { throw 'Unable to identify coverage source revision.' }
    $root = (Get-Location).Path
    $inventory = @(Get-ChildItem src, examples -Recurse -File -Filter '*.cs' |
        Where-Object { $_.FullName -notmatch '[/\\](bin|obj)[/\\]' } |
        ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/') })
    Test-CoverReport $output $revision $inventory $toolExit $Started | Out-Null
    Write-Output 'Complete coverage tool check passed for both production libraries.'
}
if ($MyInvocation.InvocationName -ne '.') {
    try { Invoke-CoverCheck $Directory $BaseRevision $NotBefore }
    catch {
        [Console]::Error.WriteLine($_)
        if ($script:CoverageToolExitCode -gt 0) { exit $script:CoverageToolExitCode }
        exit 2
    }
}
