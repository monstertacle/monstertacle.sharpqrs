. "$PSScriptRoot/Assertions.ps1"
. "$PSScriptRoot/../../scripts/check-packages.ps1"
$temporary = Join-Path $HOME "dev/.agenttemp/sharpqrs-feed & space-$([guid]::NewGuid().ToString('N'))"
New-Item $temporary -ItemType Directory -Force | Out-Null
try {
    $path = Join-Path $temporary 'NuGet.Config'
    $feed = Join-Path $temporary 'local packages & test'
    Write-ConsumerNuGetConfig $path $feed
    Assert-True (Test-Path -LiteralPath $path) 'The consumer NuGet configuration must be created.'
    [xml]$config = Get-Content -LiteralPath $path -Raw
    $sources = @($config.configuration.packageSources.add)
    Assert-True ($sources.Count -eq 2) 'Exactly the local feed and nuget.org are permitted.'
    Assert-True ($sources[0].value -ceq $feed) 'Local paths with spaces and XML metacharacters must survive unchanged.'
    Assert-True ($sources[1].value -ceq 'https://api.nuget.org/v3/index.json') 'The HTTPS source must remain a URI.'
    Assert-True ($null -ne $config.configuration.packageSources.clear) 'Inherited package sources must be cleared.'
    Assert-True ($config.configuration.auditSources.add.value -ceq 'https://api.nuget.org/v3/index.json') 'The audit source must be explicit.'
} finally { Remove-Item -LiteralPath $temporary -Recurse -Force }
Write-Output 'Package-consumer configuration cases passed.'
