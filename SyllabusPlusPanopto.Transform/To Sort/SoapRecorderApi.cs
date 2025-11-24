using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public  class SoapRecorderApi : IRecorderApi
    {
        private readonly IRemoteRecorderManagementWrapper _rrm;

        public SoapRecorderApi(IRemoteRecorderManagementWrapper rrm)
        {
            _rrm = rrm ?? throw new ArgumentNullException(nameof(rrm));
        }

        public Task<RecorderInfo?> GetByNameAsync(string recorderName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(recorderName))
                throw new ArgumentException("Recorder name is required", nameof(recorderName));

            // Thin pass through to your existing wrapper
            var settings = _rrm.GetSettingsByRecorderName(recorderName);

            if (settings is null)
            {
                // no recorder with that name
                return Task.FromResult<RecorderInfo?>(null);
            }

            // Adjust this mapping to whatever RecorderInfo actually contains
            var info = new RecorderInfo(
                settings.RecorderId,
                recorderName
            );

            return Task.FromResult<RecorderInfo?>(info);
        }
    }
}
