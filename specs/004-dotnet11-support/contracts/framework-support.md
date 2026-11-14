# Framework support contract

Public C# contracts and behavioural semantics are unchanged. Both libraries provide
net10.0 and net11.0 assets in the existing package identities. Integration depends
on DI abstractions of the corresponding major version; core remains independent.

The verifier supports both targets together. Missing evidence, duplicates or any
failed target prevent success. Runtime examples use explicit framework selection.
Release selection accepts only a clean successful root manifest listing both targets.
Package publication remains a separately authorised operation.
