param([string]$ReportPath, [datetime]$NotBefore = [datetime]::MinValue)
$ErrorActionPreference = 'Stop'
function Test-MetricsReport {
    param([string]$Path, [string[]]$Inventory, [int]$ExitCode = 0)
    if ($ExitCode -ne 0) { throw "Codemetr guard failed with exit $ExitCode." }
    $report = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
    if ($report.schema -ne 1 -or $report.tool -ne 'codemetr-csharp' -or $report.version -ne '0.1.0' -or $report.command -ne 'check') {
        throw 'Unsupported metrics report.'
    }
    if (-not $report.passed -or $report.summary.files -le 0 -or $report.types.Count -eq 0) { throw 'Empty or failed metrics report.' }
    if ($report.parseErrors.Count -gt 0 -or $report.summary.parseErrors -ne 0 -or $report.summary.skipped -ne 0) { throw 'Incomplete metrics parsing.' }
    if (-not $report.ContainsKey('completeness')) { throw 'Metrics completeness evidence is missing.' }
    foreach ($key in @('unmeasuredPartialDeclarations', 'inactiveFiles', 'contextFilesWithLimits')) {
        if (-not $report.completeness.ContainsKey($key)) { throw "Missing completeness field: $key." }
        if (@($report.completeness[$key]).Where({ $null -ne $_ }).Count -gt 0) { throw "Incomplete measurement: $key." }
    }
    foreach ($type in $report.types) {
        foreach ($declaration in $type.declarations) {
            if ($declaration.measured -eq $false) { throw 'Unmeasured hand-written declaration.' }
        }
    }
    $measured = @($report.types.file | ForEach-Object { $_.Replace('\', '/') } | Sort-Object -Unique)
    $expected = @($Inventory | ForEach-Object { $_.Replace('\', '/') } | Sort-Object -Unique)
    if ($expected.Count -eq 0 -or $report.summary.files -ne $expected.Count -or (Compare-Object $expected $measured)) {
        throw 'Metrics source inventory does not match the hand-written source scope.'
    }
    # The tool uses a synthetic compilation with platform references only. These
    # external-symbol diagnostics are reviewed alongside the real warning-free build.
    $known = @('CS0103', 'CS0234', 'CS0246', 'CS1061')
    foreach ($id in $report.completeness.compilationErrorIds) {
        if ($id -notin $known) { throw "Unreviewed synthetic-compilation diagnostic: $id." }
    }
    return $true
}
function Invoke-MetricsCheck {
    param([string]$OutputPath)
    if (Test-Path -LiteralPath $OutputPath) { throw 'Metrics output must be fresh.' }
    if (-not (Test-Path -LiteralPath (Split-Path $OutputPath -Parent))) { throw 'Metrics output directory is missing.' }
    $started = [datetime]::UtcNow
    $report = & dotnet tool run codemetr-csharp -- check src tests examples --include-metrics --format json
    $script:MetricsExitCode = $LASTEXITCODE
    $report | Set-Content -LiteralPath $OutputPath -Encoding utf8
    $root = (Get-Location).Path
    $inventory = @(Get-ChildItem src, tests, examples -Recurse -File -Filter '*.cs' |
        Where-Object { $_.FullName -notmatch '[/\\](bin|obj)[/\\]' } |
        ForEach-Object { [IO.Path]::GetRelativePath($root, $_.FullName).Replace('\', '/') })
    if ((Get-Item $OutputPath).LastWriteTimeUtc -lt $started) { throw 'Stale metrics report.' }
    Test-MetricsReport $OutputPath $inventory $script:MetricsExitCode | Out-Null
    Write-Output "Codemetr passed for $($inventory.Count) hand-written C# files; advisory metrics require review."
}
if ($MyInvocation.InvocationName -ne '.') {
    try { Invoke-MetricsCheck $ReportPath }
    catch {
        [Console]::Error.WriteLine($_)
        if ($script:MetricsExitCode -gt 0) { exit $script:MetricsExitCode }
        exit 2
    }
}
