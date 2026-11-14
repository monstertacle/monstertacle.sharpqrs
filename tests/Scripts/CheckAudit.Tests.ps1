. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/check-audit.ps1"
$fixtures = "$PSScriptRoot/Fixtures/Audit"
foreach ($name in @('vulnerable', 'incomplete', 'no-source', 'wrong-version', 'wrong-framework', 'error', 'malformed', 'absent')) {
    Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" "$fixtures/$name.json" @('example.csproj') }
}
Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" "$fixtures/clean.json" @('example.csproj') 'warning NU1900: Unable to obtain vulnerability data' }
Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" "$fixtures/clean.json" @('another.csproj') }
Assert-True (Test-AuditReports "$fixtures/inventory.json" "$fixtures/clean.json" @('example.csproj')) 'Expected a complete clean report.'
Write-Output 'Audit checker cases passed, including zero-exit vulnerability findings.'

$temporary = Join-Path $HOME "dev/.agenttemp/sharpqrs-multitarget-audit-$([guid]::NewGuid().ToString('N'))"
New-Item $temporary -ItemType Directory -Force | Out-Null
try {
    $report = Get-Content "$fixtures/clean.json" -Raw | ConvertFrom-Json -AsHashtable
    $path = Join-Path $temporary 'audit.json'
    $report.projects[0].frameworks = @($report.projects[0].frameworks[0])
    $report | ConvertTo-Json -Depth 20 | Set-Content $path
    Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" $path @('example.csproj') }
    $report.projects[0].frameworks += @{ framework = 'net11.0'; topLevelPackages = @(@{
        id = 'Example'; resolvedVersion = '1.0.0'; vulnerabilities = @(@{ severity = 'Low' })
    }) }
    $report | ConvertTo-Json -Depth 20 | Set-Content $path
    Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" $path @('example.csproj') }
    $report.projects[0].frameworks[1].topLevelPackages = @()
    $report.projects[0].frameworks += @{ framework = 'net11.0'; topLevelPackages = @() }
    $report | ConvertTo-Json -Depth 20 | Set-Content $path
    Assert-Rejected { Test-AuditReports "$fixtures/inventory.json" $path @('example.csproj') }
} finally { Remove-Item -LiteralPath $temporary -Recurse -Force }
Write-Output 'Missing, duplicate and vulnerable second-target audit cases passed.'
