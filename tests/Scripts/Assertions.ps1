$ErrorActionPreference = 'Stop'
function Assert-Rejected {
    param([scriptblock]$Action)
    $rejected = $false
    try { & $Action | Out-Null } catch { $rejected = $true }
    if (-not $rejected) { throw 'Expected invalid evidence to be rejected.' }
}
function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}
