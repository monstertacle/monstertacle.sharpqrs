. "$PSScriptRoot/package-version.ps1"
function Get-VerifiedReleasePackages {
    param([string]$Directory, [string]$Version, [string]$SourceRevision)
    if ([string]::IsNullOrWhiteSpace($Version)) { throw 'A release version is required.' }
    $Version = Resolve-PackageVersion $Version
    if ($SourceRevision -cnotmatch '\A[0-9a-f]{40}\z') { throw 'A full source commit SHA is required.' }
    $manifests = @(Get-ChildItem -LiteralPath $Directory -Recurse -File -Filter 'run.json')
    if ($manifests.Count -ne 1) { throw 'Expected exactly one verification run.' }
    $manifest = Get-Content -LiteralPath $manifests[0].FullName -Raw | ConvertFrom-Json -AsHashtable
    if ($manifest.status -cne 'passed' -or $manifest.revision -cne $SourceRevision -or $manifest.packageVersion -cne $Version) {
        throw 'Verification must have passed for this exact source revision and package version.'
    }
    if (@($manifest.workingTree | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count -gt 0) {
        throw 'Release verification must use a clean checkout.'
    }
    $feed = Join-Path $manifests[0].DirectoryName 'packages'
    $packages = @(Get-ChildItem -LiteralPath $feed -File -Filter '*.nupkg')
    if ($packages.Count -ne 2) { throw 'Expected exactly the two verified production packages.' }
    foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
        $selected = @($packages | Where-Object Name -CEQ "$name.$Version.nupkg")
        if ($selected.Count -ne 1) { throw "Missing verified package $name.$Version.nupkg." }
        $selected[0]
    }
}
