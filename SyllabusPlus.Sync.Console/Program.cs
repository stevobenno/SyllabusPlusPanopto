using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyllabusPlusPanopto.Integration;
using SyllabusPlusPanopto.Shared;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Console logging
        services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddSimpleConsole(o =>
            {
                o.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                o.TimestampFormat = "[HH:mm:ss] ";
            });
        });

        // Shared integration bootstrap
        services.AddSyllabusPlusPanoptoSync(ctx.Configuration);
    })
    .Build();

var log = host.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("ConsoleHost");

log.LogInformation("Starting SyllabusPlus → Panopto sync...");

try
{
    var orchestrator = host.Services.GetRequiredService<TimetabledEventSyncOrchestrator>();

    // Run a single sync cycle
    await orchestrator.RunAsync();

    log.LogInformation("Sync completed successfully.");
}
catch (Exception ex)
{
    log.LogError(ex, "The sync process failed.");
}
