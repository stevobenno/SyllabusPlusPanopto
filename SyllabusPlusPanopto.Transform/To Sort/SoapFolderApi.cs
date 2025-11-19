using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.ApiWrappers;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort;

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
