using Monstertacle.SharpQrs.AspNetCore;

namespace Monstertacle.SharpQrs.Examples.Web;

internal static class ExampleComposition
{
    internal static IServiceProviderFactory<IServiceCollection> CreateFactory()
        => new SharpQrsServiceProviderFactory(new DefaultServiceProviderFactory(
            new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }));

    internal static void ConfigureWeb(IServiceCollection services)
    {
        services.AddScoped<RequestScope>();
        services.AddScoped<ICommandHandler<EchoCommand, EchoResult>, EchoCommandHandler>();
        services.AddScoped<IQueryHandler<EchoQuery, EchoResult>, EchoQueryHandler>();
        services.AddSharpQrsCommand<EchoCommand, EchoResult>();
        services.AddSharpQrsQuery<EchoQuery, EchoResult>();
    }
}
