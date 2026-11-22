# Staged secret protection

Adopt SP-01 through SP-04 from the [public rollout specification](https://github.com/monstertacle/monstertacle.hosting/blob/main/specs/011-staged-secret-protection/spec.md).
SharpQrs owns rollout T007 and verification T008.

## Acceptance

A contributor reviews the exact index diff and runs the check before each
commit. Clean staged input and an unstaged synthetic value pass. Staging that
value fails with a redacted diagnostic, and scanner errors block the commit.
Verify both GitHub protection flags and the existing Linux and Windows checks.

## Clarifications

The change concerns the contributor workflow and leaves published library
contracts outside its scope. No clarification remains open.
