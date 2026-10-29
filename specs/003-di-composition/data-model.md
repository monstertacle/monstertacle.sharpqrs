# Composition data model

Mapping definitions retain request identity and typed installation delegates, plus
contract selection requirements expressed as immutable service types. No definition
retains a mutable service collection. Owned concrete service metadata records the
original ServiceDescriptor so final validation can check identity, not merely type.

A prepared composition contains immutable selected-service evidence derived from
the actual final descriptor sequence. Preparation creates a distinct read-only
collection and replaces any prepared evidence copied from another collection.
Each provider therefore receives only its preparation's evidence. All descriptor
validation precedes handler activation. Runtime adapters remain non-owning and
resolve each call from their current scope, preserving transient semantics.
