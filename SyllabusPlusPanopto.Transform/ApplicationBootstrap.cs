using Microsoft.Extensions.DependencyInjection;
using SyllabusPlusPanopto.Transform;

namespace SyllabusPlusPanopto.Application;

/// <summary>
/// DI registration for the process flow only. No other services are registered here.
/// </summary>
public static class ApplicationBootstrap
{
    public static IServiceCollection AddProcessFlow(this IServiceCollection services)
    {
        // Only the wrapper itself — assumes Reader/Transform/Sync are registered elsewhere:
        //  - Reader is added via Infrastructure.AddSourceFromConfiguration(...)
        //  - Transform and Sync are added by their own projects' bootstrappers
        services.AddSingleton<IProcessFlow, ProcessFlow>();
        return services;
    }
}
