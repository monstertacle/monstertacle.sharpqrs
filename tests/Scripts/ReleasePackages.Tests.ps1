. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/release-packages.ps1"
$temporary = Join-Path $HOME "dev/.agenttemp/sharpqrs-release-tests-$([guid]::NewGuid().ToString('N'))"
$script:releaseFailures = [Collections.Generic.List[string]]::new()
function Assert-ReleaseRejected {
    param([scriptblock]$Action)
    try { Assert-Rejected $Action } catch { $script:releaseFailures.Add($_.ToString()) }
}
$revision = 'a' * 40
try {
    $runDirectory = Join-Path $temporary 'verify-run'
    $packages = Join-Path $runDirectory 'packages'
    New-Item $packages -ItemType Directory -Force | Out-Null
    foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
        # These controlled files test selection only, not package contents.
        Set-Content "$packages/$name.1.2.3-rc.1.nupkg" 'selection fixture'
    }
    $manifest = @{ status = 'passed'; revision = $revision; packageVersion = '1.2.3-rc.1'; workingTree = $null }
    $manifest | ConvertTo-Json | Set-Content "$runDirectory/run.json"
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' ('b' * 40) }
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.2' $revision }
    $manifest.status = 'running'
    $manifest | ConvertTo-Json | Set-Content "$runDirectory/run.json"
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' $revision }
    $manifest.status = 'passed'; $manifest.workingTree = @(' M source.cs')
    $manifest | ConvertTo-Json | Set-Content "$runDirectory/run.json"
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' $revision }
    $manifest.workingTree = $null
    $manifest | ConvertTo-Json | Set-Content "$runDirectory/run.json"
    $selected = @(Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' $revision)
    try { Assert-True ($selected.Count -eq 2) 'Exactly both verified packages must be selected.' } catch { $script:releaseFailures.Add($_.ToString()) }
    try { Assert-True ($selected[0].Name -ceq 'Monstertacle.SharpQrs.1.2.3-rc.1.nupkg') 'The core must be published first.' } catch { $script:releaseFailures.Add($_.ToString()) }
    Set-Content "$packages/Unrelated.1.2.3.nupkg" 'unexpected package'
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' $revision }
    Remove-Item "$packages/Unrelated.1.2.3.nupkg"
    Remove-Item "$packages/Monstertacle.SharpQrs.AspNetCore.1.2.3-rc.1.nupkg"
    Assert-ReleaseRejected { Get-VerifiedReleasePackages $temporary '1.2.3-rc.1' $revision }
} finally { Remove-Item -LiteralPath $temporary -Recurse -Force }
if ($script:releaseFailures.Count) { throw ($script:releaseFailures -join "`n") }
Write-Output 'Verified release package selection cases passed.'
