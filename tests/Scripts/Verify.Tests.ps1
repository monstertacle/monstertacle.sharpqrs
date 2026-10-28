. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/verify.ps1"
Assert-Rejected { Assert-VerificationScripts "$PSScriptRoot/Fixtures/Nonexistent" }
$script:seen = [Collections.Generic.List[string]]::new()
Invoke-VerificationStages { param($stage) $script:seen.Add($stage); return 0 }
Assert-True ($script:seen.Count -eq 9) 'All nine verification stages must execute.'
Assert-True ('audit' -in $script:seen -and 'packages' -in $script:seen) 'Audit and packages are mandatory.'
$script:seen.Clear()
Assert-Rejected { Invoke-VerificationStages { param($stage) $script:seen.Add($stage); return 1 } }
Assert-True ($script:seen.Count -eq 1) 'A failed stage must stop orchestration.'
Write-Output 'Verification orchestration cases passed.'
