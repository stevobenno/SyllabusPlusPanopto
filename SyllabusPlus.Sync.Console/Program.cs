// SyllabusPlusPanopto.Console.Sync/Program.cs
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SyllabusPlusPanopto.Infrastructure;
using SyllabusPlusPanopto.Transform;
using SyllabusPlusPanopto.Transform.Interfaces; // AddProcessFlow, IProcessFlow
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.Telemetry;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(c =>
    {
        c.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Common infra (http factory, options) + source provider chosen via config (Csv | SqlView | Api)
        services
            .AddSyllabusPlusCommon(ctx.Configuration)
            .AddSourceFromConfiguration(ctx.Configuration);

        // Register only the process flow wrapper (no business logic here)
      

        // inside ConfigureServices
        services.AddSingleton<IIntegrationTelemetry>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>().GetSection("Telemetry");
            var useAi = config.GetValue<bool>("UseAppInsights");
            var useTeams = config.GetValue<bool>("UseTeams");
            var sinks = new List<IIntegrationTelemetry>();

            if (useAi)
            {
                var tc = new Microsoft.ApplicationInsights.TelemetryClient(
                    sp.GetRequiredService<TelemetryConfiguration>());
                sinks.Add(new AppInsightsIntegrationTelemetry(tc));
            }

            if (useTeams)
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var http = httpClientFactory.CreateClient("teams-telemetry");
                var webhook = config.GetValue<string>("TeamsWebhookUrl");
                sinks.Add(new TeamsWebhookIntegrationTelemetry(http, webhook));
            }

            // fall back to a no-op if nothing is enabled
            if (sinks.Count == 0)
                sinks.Add(new NoopIntegrationTelemetry());

            return new CompositeIntegrationTelemetry(sinks);
        });

        // also remember
        services.AddHttpClient("teams-telemetry");

    })
    .Build();

// Parse --dryRun without pulling in LINQ
var dryRun = Array.Exists(args, a => string.Equals(a, "--dryRun", StringComparison.OrdinalIgnoreCase));

// Execute the shared Read → Transform → Sync flow
await host.Services.GetRequiredService<IProcessFlow>().RunAsync(dryRun);
