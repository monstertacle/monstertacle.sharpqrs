# Dual-target validation

Install SDKs 11.0.100 and 10.0.403, including their runtimes, and PowerShell 7.
From the repository root:

```powershell
dotnet --version # 11.0.100
pwsh -NoProfile -File scripts/verify.ps1
```

On this Linux development host run heavy commands through
`bash /home/user1/dev/scripts/run-capped.sh -- pwsh -NoProfile -File scripts/verify.ps1`.

Expected: both target suites pass, independent library coverage floors and complete
coverage checks pass, metrics/audit and published compatibility pass, both sets of
examples and four package consumers run. Inspect artifacts/verify-*/run.json,
net10.0/, net11.0/ and packages/. No package is published by verification.

For the dedicated SDK installation on this host, use the environment prefix in
[development guidance](../../docs/development.md#dual-target-verification).
