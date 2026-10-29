# Validation guide

Use SDK 10.0.403 and PowerShell 7. Restore pinned tools with dotnet tool restore.
Run dotnet tool run codemetr-csharp-cover doctor --format text, then execute
pwsh -NoProfile -File scripts/verify.ps1. On the shared machine wrap heavy commands
with bash ~/dev/scripts/run-capped.sh --.

Run the standalone and web examples through verification. The standalone example
must demonstrate direct synchronous results alongside awaited asynchronous results.
Scoped examples must select existing handler contracts. Inspect fresh artifacts for
both library line floors, per-member and diff coverage, report completeness, test
counts, tool versions, source SHA and comparison base. Negative script fixtures must
reject malformed, missing, stale, partial and failed evidence. No baseline is needed.
