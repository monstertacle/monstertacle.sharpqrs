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
