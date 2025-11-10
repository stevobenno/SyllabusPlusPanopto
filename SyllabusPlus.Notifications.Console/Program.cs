
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(b => b.AddConsole());
        // TODO: add shared, domain, infrastructure registrations here
    })
    .Build();

var log = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Console");
log.LogInformation("Console host started. Add your runner here.");
