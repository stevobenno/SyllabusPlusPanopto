using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

internal sealed class SoapFolderApi : IFolderApi
{
    private readonly SessionManagementWrapper _sm;
    public SoapFolderApi(SessionManagementWrapper sm) => _sm = sm;

    public Task<FolderInfo?> GetByNameAsync(string folderName, CancellationToken ct)
    {
        var f = _sm.GetFolderByName(folderName);
        return Task.FromResult(f is null ? null : new FolderInfo(f.Id, f.Name) as FolderInfo);
    }

    public Task<FolderInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var f = _sm.GetFolderById(id);
        return Task.FromResult(f is null ? null : new FolderInfo(f.Id, f.Name) as FolderInfo);
    }
}
