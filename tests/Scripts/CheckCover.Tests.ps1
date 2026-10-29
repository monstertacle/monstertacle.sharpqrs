. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/check-cover.ps1"
$scratch = Join-Path ([IO.Path]::GetTempPath()) "sharpqrs-cover-fixtures-$([guid]::NewGuid().ToString('N'))"
New-Item $scratch -ItemType Directory | Out-Null
try {
    $path = Join-Path $scratch 'cover.json'
    $revision = 'abcdef1234567890123456789012345678901234'
    $clean = @{
        schema = 1; tool = 'codemetr-csharp-cover'; version = '0.1.1'; command = 'check'; passed = $true; complete = $true
        provenance = @{ source = 'reports'; head = $revision.Substring(0, 7); unreported = 'zero'; partial = @()
            diff = @{ base = 'origin/main'; mergeBase = 'abcdef1'; head = $revision.Substring(0, 7) }
            reports = @('coverage.cobertura.xml')
            testProjects = @(@{ project = 'tests/Tests.csproj'; tests = @{ total = 1; passed = 1; failed = 0; skipped = 0 } }) }
        summary = @{ files = 1; coverableLines = 1; violations = 0; unreported = 0; unmapped = 0 }
        types = @(@{ file = 'src/Example.cs' }); parseErrors = @(); inputErrors = @(); violations = @(); stale = @(); unreported = @()
    }
    function Write-Fixture($value) { $value | ConvertTo-Json -Depth 12 | Set-Content $path }
    Write-Fixture $clean
    Assert-True (Test-CoverReport $path $revision @('src/Example.cs')) 'Valid complete coverage should pass.'
    foreach ($exitCode in @(1, 2, 127)) { Assert-Rejected { Test-CoverReport $path $revision @('src/Example.cs') $exitCode } }
    foreach ($mutation in @(
        { param($d) $d.complete = $false }, { param($d) $d.passed = $false },
        { param($d) $d.version = '0.1.0' }, { param($d) $d.provenance.head = '1234567' },
        { param($d) $d.provenance.partial = @('filtered') }, { param($d) $d.inputErrors = @('staleCoverage') },
        { param($d) $d.parseErrors = @('invalid') }, { param($d) $d.summary.files = 0 },
        { param($d) $d.summary.coverableLines = 0 }, { param($d) $d.summary.unreported = 1 },
        { param($d) $d.summary.unmapped = 1 }, { param($d) $d.summary.violations = 1 },
        { param($d) $d.types = @() }, { param($d) $d.provenance.diff.mergeBase = '' },
        { param($d) $d.provenance.testProjects[0].tests.failed = 1 },
        { param($d) $d.provenance.testProjects[0].tests.total = 0 },
        { param($d) $d.provenance.testProjects = @() }, { param($d) $d.Remove('complete') }
    )) {
        $value = $clean | ConvertTo-Json -Depth 12 | ConvertFrom-Json -AsHashtable
        & $mutation $value
        Write-Fixture $value
        Assert-Rejected { Test-CoverReport $path $revision @('src/Example.cs') }
    }
    Write-Fixture $clean
    Assert-Rejected { Test-CoverReport $path $revision @('src/Other.cs') }
    Assert-Rejected { Test-CoverReport $path $revision @('src/Example.cs') 0 ([datetime]::UtcNow.AddMinutes(1)) }
    Set-Content $path '{invalid'
    Assert-Rejected { Test-CoverReport $path $revision @('src/Example.cs') }
    Assert-Rejected { Test-CoverReport (Join-Path $scratch 'missing.json') $revision @('src/Example.cs') }
    Write-Output 'Coverage tool evidence cases passed.'
} finally { Remove-Item $scratch -Recurse -Force }
