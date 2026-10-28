param([string]$ReportPath, [datetime]$NotBefore = [datetime]::MinValue)
$ErrorActionPreference = 'Stop'
function Test-CoverageReport {
    param([string]$Path, [datetime]$Started = [datetime]::MinValue)
    $file = Get-Item -LiteralPath $Path
    if ($file.LastWriteTimeUtc -lt $Started.ToUniversalTime()) { throw 'Stale coverage report.' }
    [xml]$report = Get-Content -LiteralPath $Path -Raw
    if ($Started -gt [datetime]::MinValue) {
        $timestamp = 0L
        if (-not [long]::TryParse($report.coverage.timestamp, [ref]$timestamp)) { throw 'Coverage generation timestamp is missing.' }
        $generated = [DateTimeOffset]::FromUnixTimeSeconds($timestamp).UtcDateTime
        if ($generated -lt $Started.ToUniversalTime().AddSeconds(-1)) { throw 'Stale coverage generation timestamp.' }
    }
    $expected = @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')
    $packages = @($report.coverage.packages.package)
    if ($packages.Count -ne 2) { throw 'Coverage must contain exactly both production assemblies.' }
    foreach ($name in $expected) {
        $package = @($packages | Where-Object name -eq $name)
        if ($package.Count -ne 1) { throw "Missing or duplicate coverage for $name." }
        $lines = @($package[0].classes.class | ForEach-Object { $_.lines.line })
        if ($lines.Count -eq 0) { throw "No executable lines measured for $name." }
        $identities = @{}
        foreach ($class in $package[0].classes.class) {
            if ([string]::IsNullOrWhiteSpace($class.filename)) { throw 'Coverage source path is missing.' }
            foreach ($line in $class.lines.line) {
                if (-not $line.HasAttribute('number') -or -not $line.HasAttribute('hits')) { throw 'Malformed coverage line.' }
                if ([int]$line.number -le 0 -or [int]$line.hits -lt 0) { throw 'Invalid coverage line.' }
                $key = "$($class.filename):$($line.number)"
                $identities[$key] = [Math]::Max([int]$identities[$key], [int]$line.hits)
            }
        }
        if ($identities.Count -eq 0) { throw "No executable lines measured for $name." }
        $covered = @($identities.Values | Where-Object { $_ -gt 0 }).Count
        $percentage = 100.0 * $covered / $identities.Count
        if ($percentage -lt 90) { throw "$name has $percentage percent line coverage; at least 90 is required." }
        $branchCovered = 0; $branchValid = 0
        foreach ($line in $lines) {
            if ($line.'condition-coverage' -match '\((\d+)/(\d+)\)') {
                $branchCovered += [int]$Matches[1]; $branchValid += [int]$Matches[2]
            }
        }
        [pscustomobject]@{ Assembly = $name; LinesCovered = $covered; LinesValid = $identities.Count
            LineCoverage = $percentage; BranchesCovered = $branchCovered; BranchesValid = $branchValid }
    }
}
if ($MyInvocation.InvocationName -ne '.') { Test-CoverageReport $ReportPath $NotBefore }
