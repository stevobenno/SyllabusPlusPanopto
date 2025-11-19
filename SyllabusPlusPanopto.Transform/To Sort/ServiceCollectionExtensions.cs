using Microsoft.Extensions.DependencyInjection;
using SyllabusPlusPanopto.Transform.Interfaces;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort
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
