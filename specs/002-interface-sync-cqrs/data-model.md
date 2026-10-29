# Registration model

A dispatcher owns a setup-only dictionary keyed by exact runtime request type.
Each registration records exact result Type and a typed invocation Delegate.
Command/query and synchronous/asynchronous registries are independent. Successful
setup creates one mapping; duplicate setup leaves the original unchanged. Dispatch
reads stable metadata and never mutates it. Standalone handlers remain caller-owned.

Integration metadata installs non-owning adapters into a scope-local dispatcher.
Contract service descriptors are checked for uniqueness without activation. Provider
construction ends configuration. Scope disposal releases container-owned resources;
adapters and dispatchers never dispose selected handlers.
