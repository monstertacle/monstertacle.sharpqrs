. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/package-version.ps1"
$failures = [Collections.Generic.List[string]]::new()
foreach ($version in @('0.1.0', '0.1.0-alpha.1', '10.20.30-rc.2', '1.0.0-beta-x')) {
    try { Assert-True ((Resolve-PackageVersion $version) -ceq $version) "Valid version changed: $version" }
    catch { $failures.Add($_.ToString()) }
}
foreach ($version in @('v1.0.0', '01.0.0', '1.2', '1.0.0+metadata', '1.0.0-01', '1.0.0-alpha.01', '1.0.0-', '1.0.0;a', "1.0.0`n")) {
    try { Assert-Rejected { Resolve-PackageVersion $version } }
    catch { $failures.Add("Invalid version accepted: $version") }
}
$temporary = Join-Path $HOME "dev/.agenttemp/sharpqrs-version-tests-$([guid]::NewGuid().ToString('N'))"
try {
    foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
        $directory = Join-Path $temporary "src/$name"
        New-Item $directory -ItemType Directory -Force | Out-Null
        Set-Content "$directory/$name.csproj" '<Project><PropertyGroup><Version>7.8.9-alpha.1</Version></PropertyGroup></Project>'
    }
    try { Assert-True ((Resolve-PackageVersion '' $temporary) -ceq '7.8.9-alpha.1') 'Matching project defaults were not resolved.' }
    catch { $failures.Add($_.ToString()) }
    Set-Content "$temporary/src/Monstertacle.SharpQrs.AspNetCore/Monstertacle.SharpQrs.AspNetCore.csproj" '<Project><PropertyGroup><Version>7.8.10</Version></PropertyGroup></Project>'
    try { Assert-Rejected { Resolve-PackageVersion '' $temporary } }
    catch { $failures.Add('Mismatched project versions were accepted.') }
} finally { Remove-Item -LiteralPath $temporary -Recurse -Force }
if ($failures.Count) { throw ($failures -join "`n") }
Write-Output 'Package version validation and default-resolution cases passed.'
