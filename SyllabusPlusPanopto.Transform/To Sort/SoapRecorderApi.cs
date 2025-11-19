using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.ApiWrappers;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort;

internal sealed class SoapRecorderApi : IRecorderApi
{
    private readonly RemoteRecorderManagementWrapper _rrm;
    public SoapRecorderApi(RemoteRecorderManagementWrapper rrm) => _rrm = rrm;


    public Task<RecorderInfo> GetByNameAsync(string recorderName, CancellationToken ct)
    {
        throw new System.NotImplementedException();
    }
}
