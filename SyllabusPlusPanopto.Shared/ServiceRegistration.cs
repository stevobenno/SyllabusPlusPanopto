using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyllabusPlusPanopto.Integration;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Implementations;
using SyllabusPlusPanopto.Integration.Interfaces;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;
using SyllabusPlusPanopto.Integration.To_Sort;
using SyllabusPlusPanopto.Integration.TransformationServices;
using System;


// SoapPanoptoPlatform etc.

namespace SyllabusPlusPanopto.Shared;

public static class SyncServiceRegistration
{
    public static IServiceCollection AddSyllabusPlusPanoptoSync(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Bind options
        services.Configure<SourceOptions>(config.GetSection("Source"));
        services.Configure<PanoptoOptions>(config.GetSection("Panopto"));

        // Decide input source (this is where your SourceKind enum is used)
        var sourceKind = config.GetValue<SourceKind>("Source:Kind");

        switch (sourceKind)
        {
            case SourceKind.Csv:
                services.AddSingleton<ISourceDataProvider, CsvSourceProvider>();
                break;

            case SourceKind.Sql:
                services.AddSingleton<ISourceDataProvider, SqlViewSourceProvider>();
                break;

            case SourceKind.Api:
                services.AddHttpClient<ISourceDataProvider, ApiSourceProvider>();
                break;

            default:
                throw new InvalidOperationException($"Unsupported SourceKind '{sourceKind}'.");
        }

        // Panopto SOAP wrappers (your existing classes)
        services.AddSingleton<ISessionManagementWrapper, SessionManagementWrapper>();
        services.AddSingleton<IRemoteRecorderManagementWrapper, RemoteRecorderManagementWrapper>();
        // or just the wrappers themselves if they’re not factories yet

        // Higher-level abstraction
        services.AddSingleton<IPanoptoPlatform, SoapPanoptoPlatform>();

        // Orchestrator that the host actually calls
        services.AddSingleton<TimetabledEventSyncOrchestrator>();


        // if then logic
        services.AddSingleton<ITransformService, RouteOneTransformService>();
        // sexy automapper transformation
        //services.AddSingleton<ITransformService, AutoMapperTransformService>();

        services.AddSingleton<ITimetabledEventSyncOrchestrator, TimetabledEventSyncOrchestrator>();
        services.AddSingleton<ISyncService, PanoptoSyncService>();


        services.AddSingleton<IPanoptoBindingFactory, PanoptoBindingFactory>();
        services.AddSingleton<ISessionManagementWrapper, SessionManagementWrapper>();
        services.AddSingleton<IRemoteRecorderManagementWrapper, RemoteRecorderManagementWrapper>();
        services.AddSingleton<IWorkingStore, InMemoryWorkingStore>();
        services.AddSingleton<ISessionApi, SoapSessionApi>();
        services.AddSingleton<IFolderApi, SoapFolderApi>();
        services.AddSingleton<IRecorderApi, SoapRecorderApi>(); 


        return services;
    }
}
