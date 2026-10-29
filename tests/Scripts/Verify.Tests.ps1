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

$temporary = Join-Path $HOME "dev/.agenttemp/sharpqrs-coverage-tests-$([guid]::NewGuid().ToString('N'))"
New-Item $temporary -ItemType Directory -Force | Out-Null
try {
    Assert-Rejected { Publish-CoverageReport $temporary }
    $first = Join-Path $temporary ([guid]::NewGuid().ToString())
    New-Item $first -ItemType Directory | Out-Null
    Copy-Item "$PSScriptRoot/Fixtures/Coverage/clean.xml" "$first/coverage.cobertura.xml"
    # VSTest's TRX logger mirrors the attachment under a machine-specific In folder.
    $mirror = Join-Path $temporary '_machine_run/In/machine'
    New-Item $mirror -ItemType Directory -Force | Out-Null
    Copy-Item "$first/coverage.cobertura.xml" "$mirror/coverage.cobertura.xml"
    $published = Publish-CoverageReport $temporary
    Assert-True (Test-Path -LiteralPath $published) 'The unique collector report must be published.'
    Assert-True ((Get-FileHash $published).Hash -eq (Get-FileHash "$first/coverage.cobertura.xml").Hash) 'Report contents must be preserved.'
    Assert-Rejected { Publish-CoverageReport $temporary }
    Remove-Item -LiteralPath $published
    $second = Join-Path $temporary ([guid]::NewGuid().ToString())
    New-Item $second -ItemType Directory | Out-Null
    Copy-Item "$first/coverage.cobertura.xml" "$second/coverage.cobertura.xml"
    Assert-Rejected { Publish-CoverageReport $temporary }
} finally { Remove-Item -LiteralPath $temporary -Recurse -Force }
Write-Output 'Collector report discovery cases passed.'
