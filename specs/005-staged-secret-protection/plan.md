# Implementation plan

The [Hosting rollout plan](https://github.com/monstertacle/monstertacle.hosting/blob/main/specs/011-staged-secret-protection/plan.md)
defines the shared behaviour. At source revision 4cd1e9935fe3c5e4846fd3d36834d0aff5b89edb,
Constitution 2.1.0 requires ignore rules and secret scanning but supplies no
staged scanner. The existing Verify workflow calls `scripts/verify.ps1` and
checks both target frameworks. Introduce a standalone contributor command
under `scripts/` that reads Git's index through Gitleaks 8.30.1. It emits a
redacted status. GitHub protects supported patterns at push time.

Constitution 2.2.0 applies. The public libraries, package consumers, native
tests and coverage gates have no dependency on the local scanner. A whole-tree
scan would include unstaged edits and a hosting-only gate would leave no
pre-commit check. Use the shared staged command and existing GitHub protection.

Before implementing, demonstrate that the synthetic staged change is accepted
when the check is absent. After implementation, demonstrate redacted rejection,
clean and unstaged acceptance and failure on an invalid index or dependency.
Check the Spec Kit configuration and the existing Linux and Windows Verify jobs.
