using System;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SyllabusPlus.Sync.Console;
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
log.LogInformation(
    "\n" +
    "=============================================================\n" +
    "   S Y L L A B U S   P L U S   →   P A N O P T O   S Y N C\n" +
    "=============================================================\n" +
    "  Version       : {Version}\n" +
    "  Build Date    : {BuildDate}\n" +
    "  Environment   : {Environment}\n" +
    "  Machine       : {Machine}\n" +
    "  Started At    : {Started}\n" +
    "-------------------------------------------------------------\n" +
    "  Description   : Automated Timetabled Event Synchronisation\n" +
    "                  Using Panopto SOAP + REST APIs\n" +
    "                  + Azure-Style Ephemeral Working Store\n" +
    "-------------------------------------------------------------\n" +
    "  Major Steps   :\n" +
    "      1. Initialise working store\n" +
    "      2. Query Panopto for existing sessions\n" +
    "      3. Seed in-memory session map (ExternalId-based)\n" +
    "      4. Query Syllabus+ event window\n" +
    "      5. Reconcile → Add / Update / Delete sessions\n" +
    "      6. Produce run summary\n" +
    "=============================================================\n" +
    "  Sync service initialising…\n" +
    "=============================================================",
    AppMetadata.Version,
    AppMetadata.BuildDate,
    AppMetadata.EnvironmentName,
    Environment.MachineName,
    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
);


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
