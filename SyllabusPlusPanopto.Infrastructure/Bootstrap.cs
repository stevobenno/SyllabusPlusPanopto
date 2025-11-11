using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyllabusPlusPanopto.Transform.Implementations;
using SyllabusPlusPanopto.Transform.Interfaces;
// If your concrete providers live elsewhere, adjust the usings:
// CsvSourceProvider, SqlViewSourceProvider, ApiSourceProvider

namespace SyllabusPlusPanopto.Infrastructure
{
    public static class Bootstrap
    {
        /// Common cross-cutting bits (http factory, etc.)
        public static IServiceCollection AddSyllabusPlusCommon(this IServiceCollection services, IConfiguration cfg)
        {
            services.AddHttpClient();
            // bind options for later use too
            services.Configure<SourceOptions>(cfg.GetSection("Source"));
            return services;
        }

        /// Registers ISourceDataProvider based on configuration: Source.Kind = Csv | SqlView | Api
        public static IServiceCollection AddSourceFromConfiguration(this IServiceCollection services, IConfiguration cfg)
        {
            var opts = new SourceOptions();
            cfg.GetSection("Source").Bind(opts);

            // fail fast if Kind missing
            if (opts.Kind is SourceKind.Unknown)
                throw new InvalidOperationException("Source.Kind must be Csv, SqlView, or Api.");

            switch (opts.Kind)
            {
                case SourceKind.Csv:
                    if (string.IsNullOrWhiteSpace(opts.CsvPath))
                        throw new InvalidOperationException("Source.CsvPath is required for Csv.");
                    services.AddSingleton<ISourceDataProvider>(_ => new CsvSourceProvider(opts.CsvPath!));
                    break;

                case SourceKind.SqlView:
                    if (string.IsNullOrWhiteSpace(opts.SqlConnectionString) || string.IsNullOrWhiteSpace(opts.SqlViewName))
                        throw new InvalidOperationException("Source.SqlConnectionString and Source.SqlViewName are required for SqlView.");
                    services.AddSingleton<ISourceDataProvider>(_ =>
                        new SqlViewSourceProvider(opts.SqlConnectionString!, opts.SqlViewName!));
                    break;

                case SourceKind.Api:
                    if (string.IsNullOrWhiteSpace(opts.ApiBaseUrl))
                        throw new InvalidOperationException("Source.ApiBaseUrl is required for Api.");
                    services.AddHttpClient<ApiSourceProvider>(c =>
                    {
                        c.BaseAddress = new Uri(opts.ApiBaseUrl!, UriKind.Absolute);
                        if (!string.IsNullOrWhiteSpace(opts.ApiKey))
                            c.DefaultRequestHeaders.Add("x-api-key", (IEnumerable<string>)opts.ApiKey);
                    });
                    services.AddSingleton<ISourceDataProvider>(sp => sp.GetRequiredService<ApiSourceProvider>());
                    break;
            }

            return services;
        }
    }
}
