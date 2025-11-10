// SyllabusPlusPanopto.Console.Sync/Program.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SyllabusPlusPanopto.Infrastructure.Bootstrapping;   // AddSyllabusPlusCommon, AddSourceFromConfiguration
using SyllabusPlusPanopto.Application;                    // AddProcessFlow, IProcessFlow

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
        services.AddProcessFlow();
    })
    .Build();

// Parse --dryRun without pulling in LINQ
var dryRun = Array.Exists(args, a => string.Equals(a, "--dryRun", StringComparison.OrdinalIgnoreCase));

// Execute the shared Read → Transform → Sync flow
await host.Services.GetRequiredService<IProcessFlow>().RunAsync(dryRun);
