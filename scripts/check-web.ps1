param([Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$dll = [IO.Path]::GetFullPath('examples/Web/bin/Release/net10.0/Web.dll')
$stdout = Join-Path $OutputDirectory 'web.stdout.log'
$stderr = Join-Path $OutputDirectory 'web.stderr.log'
$process = Start-Process dotnet -ArgumentList @("`"$dll`"", '--urls', 'http://127.0.0.1:0') -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
try {
    $address = $null
    $deadline = [datetime]::UtcNow.AddSeconds(30)
    while (-not $address -and [datetime]::UtcNow -lt $deadline) {
        if ($process.HasExited) { throw "Web sample exited: $(Get-Content $stderr -Raw)" }
        $text = Get-Content $stdout -Raw -ErrorAction SilentlyContinue
        if ($text -match 'Now listening on: (http://127\.0\.0\.1:\d+)') { $address = $Matches[1] }
        if (-not $address) { Start-Sleep -Milliseconds 100 }
    }
    if (-not $address) { throw 'Web sample did not become ready.' }
    $command = Invoke-RestMethod -Method Post -Uri "$address/demo/commands" -ContentType 'application/json' -Body '{"value":"example"}' -TimeoutSec 10
    $query = Invoke-RestMethod -Uri "$address/demo/queries/example" -TimeoutSec 10
    if ($command.value -ne 'example' -or $query.value -ne 'example') { throw 'Web example returned incorrect results.' }
    if (-not $command.scopeId -or -not $query.scopeId -or $command.scopeId -eq $query.scopeId) { throw 'Web requests did not have distinct scopes.' }
    Write-Output 'Web command and query smoke checks passed.'
} finally {
    if (-not $process.HasExited) { $process.Kill($true); $process.WaitForExit() }
    $process.Dispose()
}
