using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

internal sealed class SoapRecorderApi : IRecorderApi
{
    private readonly RemoteRecorderManagementWrapper _rrm;
    public SoapRecorderApi(RemoteRecorderManagementWrapper rrm) => _rrm = rrm;


    public Task<RecorderInfo> GetByNameAsync(string recorderName, CancellationToken ct)
    {
        throw new System.NotImplementedException();
    }
}
