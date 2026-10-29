# Composition research

Decision adopted after owner approval: validate actual descriptors at a checked preparation boundary;
use a provider-factory wrapper for Generic Host's final container construction.
The standard provider's public abstraction cannot count descriptors without resolving
services. IServiceProviderIsService can detect existence, not distinguish one from
two registrations. Counting GetServices activates handlers and can trigger resources
or errors. Reflection into call sites ties the library to private container details.
Neither meets the accepted constraints.

Retaining a mutable collection, taking a snapshot when mapping too early, or attaching
snapshot metadata that can be copied and changed without preparation preserves the
original mismatch. Preparing immediately before container creation handles actual
copied descriptors without requiring handlers to activate.

A required provider-specific validation step for interface mappings is the precise
unpublished integration contract change. Published concrete APIs remain compatible;
the required step is not silently imposed without the owner's decision.

Existing .NET 10 SDK and tools are reused. Relevant Spec Kit phase skills, dotnet
run-tests/platform guidance and Codemetr guard/coverage-guard sources already read
in this session apply. No new package is proposed; the wrapper takes an inner
IServiceProviderFactory from DI abstractions, supplied by the host.


Source checks: Microsoft documents ServiceCollection.MakeReadOnly in DI abstractions
and HostBuilder.UseServiceProviderFactory as the provider-factory boundary. The
integration can therefore retain its existing package dependencies. The standard
factory must receive the final configured collection, after ConfigureContainer.
The scoped worker example uses the supported callback-based Generic Host API so
its composition can be customised by callers and by the original-error regression.

References: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.servicecollection?view=net-10.0
and https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuilder?view=net-10.0.
