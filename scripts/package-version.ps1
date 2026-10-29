function Resolve-PackageVersion {
    param([string]$Version, [string]$Root = (Split-Path $PSScriptRoot -Parent))
    if ([string]::IsNullOrEmpty($Version)) {
        $defaults = foreach ($name in @('Monstertacle.SharpQrs', 'Monstertacle.SharpQrs.AspNetCore')) {
            [xml]$project = Get-Content -LiteralPath (Join-Path $Root "src/$name/$name.csproj") -Raw
            $values = @($project.Project.PropertyGroup.Version | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
            if ($values.Count -ne 1) { throw "Expected one explicit package version in $name." }
            [string]$values[0]
        }
        if ($defaults[0] -cne $defaults[1]) { throw 'Both library package versions must agree.' }
        $Version = $defaults[0]
    }
    if ($Version -cnotmatch '\A(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?\z') {
        throw 'Use a NuGet version such as 0.1.0 or 0.2.0-rc.1, without a v prefix or build metadata.'
    }
    $prerelease = $Matches[4]
    foreach ($identifier in @($prerelease -split '\.')) {
        if ($identifier -match '\A[0-9]+\z' -and $identifier.Length -gt 1 -and $identifier.StartsWith('0', [StringComparison]::Ordinal)) {
            throw 'Numeric prerelease identifiers must not contain leading zeroes.'
        }
    }
    return $Version
}
