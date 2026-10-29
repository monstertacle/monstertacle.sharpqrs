param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
function Test-AuditReports {
    param([string]$InventoryPath, [string]$AuditPath, [string[]]$ExpectedProjects, [string]$Diagnostics = '')
    if (-not [string]::IsNullOrWhiteSpace($Diagnostics)) { throw "Dependency audit diagnostics: $Diagnostics" }
    $inventory = Get-Content -LiteralPath $InventoryPath -Raw | ConvertFrom-Json -AsHashtable
    $audit = Get-Content -LiteralPath $AuditPath -Raw | ConvertFrom-Json -AsHashtable
    if ($ExpectedProjects.Count -eq 0) { throw 'Expected project inventory is empty.' }
    foreach ($report in @($inventory, $audit)) {
        if ($report.version -ne 1 -or $report.projects.Count -ne $ExpectedProjects.Count) { throw 'Incomplete or unsupported dependency report.' }
        if ($report.problems -or $report.errors -or $report.error) { throw 'Dependency report contains errors.' }
        $actual = @($report.projects.path | ForEach-Object { [IO.Path]::GetFullPath($_) } | Sort-Object)
        $expected = @($ExpectedProjects | ForEach-Object { [IO.Path]::GetFullPath($_) } | Sort-Object)
        if (Compare-Object $actual $expected) { throw 'Dependency reports do not match the solution projects.' }
        foreach ($project in $report.projects) {
            if ($project.problems -or $project.errors -or $project.error) { throw 'Project audit failed.' }
            if ($project.frameworks.Count -ne 1 -or $project.frameworks[0].framework -ne 'net10.0') { throw 'Missing or unexpected target framework.' }
            $framework = $project.frameworks[0]
            if ($framework.problems -or $framework.errors -or $framework.error) { throw 'Framework audit failed.' }
            $packages = @($framework.topLevelPackages) + @($framework.transitivePackages)
            foreach ($package in $packages) {
                if ($null -eq $package) { continue }
                if (-not $package.id -or -not $package.resolvedVersion) { throw 'Malformed package inventory.' }
                if ($package.vulnerabilities.Count -gt 0) { throw "Known vulnerability in $($package.id); all severities fail." }
            }
        }
    }
    if ('https://api.nuget.org/v3/index.json' -notin $audit.sources) { throw 'The required audit source is absent.' }
    return $true
}
function Invoke-AuditCheck {
    param([string]$Directory)
    foreach ($name in @('inventory', 'vulnerabilities')) {
        $path = Join-Path $Directory "$name.json"
        if (Test-Path -LiteralPath $path) { throw 'Audit output must be fresh.' }
        $arguments = @('package', 'list', '--project', 'Monstertacle.SharpQrs.slnx', '--include-transitive', '--no-restore', '--format', 'json', '--output-version', '1')
        if ($name -eq 'vulnerabilities') { $arguments += '--vulnerable' }
        & dotnet @arguments 2> "$path.stderr" | Set-Content -LiteralPath $path -Encoding utf8
        if ($LASTEXITCODE -ne 0) { throw "Dependency listing failed with exit $LASTEXITCODE." }
    }
    [xml]$solution = Get-Content Monstertacle.SharpQrs.slnx -Raw
    $projects = @($solution.SelectNodes('//Project') | ForEach-Object { [IO.Path]::GetFullPath($_.Path) })
    $diagnostics = (Get-Content "$Directory/inventory.json.stderr", "$Directory/vulnerabilities.json.stderr" -Raw) -join "`n"
    Test-AuditReports "$Directory/inventory.json" "$Directory/vulnerabilities.json" $projects $diagnostics | Out-Null
    $inventory = Get-Content "$Directory/inventory.json" -Raw | ConvertFrom-Json -AsHashtable
    foreach ($project in $inventory.projects) {
        $lockPath = Join-Path (Split-Path $project.path -Parent) 'packages.lock.json'
        $lock = Get-Content $lockPath -Raw | ConvertFrom-Json -AsHashtable
        $locked = @($lock.dependencies['net10.0'].GetEnumerator() | Where-Object { $_.Value.type -ne 'Project' } |
            ForEach-Object { "$($_.Key.ToLowerInvariant())=$($_.Value.resolved)" } | Sort-Object)
        $framework = $project.frameworks[0]
        $listed = @((@($framework.topLevelPackages) + @($framework.transitivePackages)) | Where-Object { $null -ne $_ } |
            ForEach-Object { "$($_.id.ToLowerInvariant())=$($_.resolvedVersion)" } | Sort-Object)
        if (($locked.Count -ne $listed.Count) -or ($locked.Count -gt 0 -and (Compare-Object $locked $listed))) {
            throw "Dependency inventory differs from the lock file: $($project.path)."
        }
    }
    Write-Output "Audit passed for all $($projects.Count) net10.0 projects against nuget.org; no known vulnerabilities reported."
}
if ($MyInvocation.InvocationName -ne '.') { Invoke-AuditCheck $OutputDirectory }
