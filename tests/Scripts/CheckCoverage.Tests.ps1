. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/check-coverage.ps1"
$fixtures = "$PSScriptRoot/Fixtures/Coverage"
foreach ($name in @('low', 'missing', 'empty', 'malformed', 'absent')) {
    Assert-Rejected { Test-CoverageReport "$fixtures/$name.xml" }
}
Assert-Rejected { Test-CoverageReport "$fixtures/clean.xml" ([datetime]::UtcNow.AddDays(1)) }
Assert-Rejected { Test-CoverageReport "$fixtures/stale.xml" ([datetime]'2020-01-01T00:00:00Z') }
$results = @(Test-CoverageReport "$fixtures/clean.xml")
Assert-True ($results.Count -eq 2) 'Both production modules must be measured.'
Assert-True ($results[0].LineCoverage -eq 100) 'Expected complete line coverage.'
Assert-True ($results[0].BranchesValid -eq 0) 'Branch totals must be reported.'
$boundary = @(Test-CoverageReport "$fixtures/boundary.xml")
Assert-True ($boundary[1].LineCoverage -eq 90) 'The inclusive 90 percent boundary must pass.'
Write-Output 'Coverage checker cases passed.'
