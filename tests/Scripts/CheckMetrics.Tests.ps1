. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/check-metrics.ps1"
$fixtures = "$PSScriptRoot/Fixtures/Metrics"
foreach ($exitCode in @(1, 2, 127)) { Assert-Rejected { Test-MetricsReport "$fixtures/clean.json" @('src/Example.cs') $exitCode } }
foreach ($name in @('empty', 'parse', 'omitted', 'inactive', 'partial', 'malformed', 'absent')) {
    Assert-Rejected { Test-MetricsReport "$fixtures/$name.json" @('src/Example.cs') 0 }
}
Assert-True (Test-MetricsReport "$fixtures/clean.json" @('src/Example.cs') 0) 'Expected valid full-scope evidence.'
Assert-Rejected { Invoke-MetricsCheck "$fixtures/missing-directory/report.json" }
Write-Output 'Metrics checker cases passed; external-symbol limits remain review items.'
