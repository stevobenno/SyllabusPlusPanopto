
namespace SyllabusPlusPanopto.Shared;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ServiceRegistration
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration config)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddOptions();
        return services;
    }
}
