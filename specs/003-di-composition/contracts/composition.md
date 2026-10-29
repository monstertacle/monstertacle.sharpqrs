# Approved checked composition boundary

The additive setup API is `IServiceCollection PrepareSharpQrs(this IServiceCollection
services)`. It validates the final collection without activating services and returns
a separate read-only collection with immutable composition metadata. Failed checks
leave the input untouched. Repeating preparation produces a fresh independent snapshot.

Direct DI composition becomes:

```csharp
using var provider = services.PrepareSharpQrs().BuildServiceProvider(options);
```

The additive `SharpQrsServiceProviderFactory` wraps an existing
`IServiceProviderFactory<IServiceCollection>`. Its constructor takes that inner
factory; CreateBuilder delegates, and CreateServiceProvider prepares the actual
final builder immediately before passing the read-only collection to the inner
factory. This retains DI-abstractions-only dependencies and supports Generic Host:

```csharp
builder.Host.UseServiceProviderFactory(
    new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory(options)));
```

Approved strict path: interface mappings require prepared metadata. Without it,
resolution throws a diagnostic directing the caller to the checked boundary,
instead of pretending to validate a collection the provider does not expose.
Existing published concrete-only registration remains supported with the ordinary
provider builder. Complete final validation for those helpers is available through
preparation as well; ordinary construction retains its existing setup-time guarantee.

The required setup change affects the unpublished interface helpers introduced on
main in feature 002; no published public method is removed. It still requires the
explicit contract decision because existing source users of those helpers must add
the preparation call or host factory. Migration guidance and focused regression
checks are mandatory. Simply retaining the old closure as a fallback would leave
both reviewed copy counterexamples unresolved and is not proposed as a complete fix.
