# Validation guide

Run the pinned SDK/PowerShell verifier through the capped runner on the shared
machine. Confirm the mode-isolation regression fails on the reviewed baseline and
passes after separating validation. Existing same-mode conflicts and shared concrete
handler disposal must remain passing.

After approval of the checked composition contract, run the full copied collection
matrix for async/sync command/query mappings, no/one/two services, keyed services,
independent providers and unchanged source collections. Verify direct preparation
and Generic Host's wrapper using real service providers with scope validation.

Run decorated and worker examples and assert one scope per unit of work, shared
command/query dependencies inside that scope, original cancellation/errors and
exactly-once asynchronous disposal. Complete all existing coverage, metrics, audit,
source/binary compatibility and isolated package gates on Windows and Linux.
