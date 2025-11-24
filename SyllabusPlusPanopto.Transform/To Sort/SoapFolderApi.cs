using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public  class SoapFolderApi : IFolderApi
{
    private readonly ISessionManagementWrapper _sm;
    public SoapFolderApi(ISessionManagementWrapper sm) => _sm = sm;

    public Task<FolderInfo?> GetFolderByQuery(string folderQuery, CancellationToken ct)
    {
        var f = _sm.GetFolderByQuery(folderQuery);
        return Task.FromResult(f is null ? null : new FolderInfo(f.Id, f.Name) as FolderInfo);
    }

    public Task<FolderInfo?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var f = _sm.GetFolderById(id);
        return Task.FromResult(f is null ? null : new FolderInfo(f.Id, f.Name) as FolderInfo);
    }
}
