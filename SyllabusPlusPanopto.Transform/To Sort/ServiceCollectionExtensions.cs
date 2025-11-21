using Microsoft.Extensions.DependencyInjection;
using SyllabusPlusPanopto.Integration.Interfaces;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPanoptoSyncInMemory(
            this IServiceCollection services,
            string panoptoUser,
            string panoptoPassword)
        {
            //services.AddSingleton<IPanoptoPlatform>(_ => new SoapPanoptoPlatform(panoptoUser, panoptoPassword));
            services.AddSingleton<IWorkingStore, InMemoryWorkingStore>(sp =>
                new InMemoryWorkingStore(sp.GetRequiredService<IPanoptoPlatform>().Sessions));
            services.AddSingleton<ISyncService, PanoptoSyncService>();
            return services;
        }
    }

}
