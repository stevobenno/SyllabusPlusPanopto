using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

internal sealed class RestFolderApi : IFolderApi
{
    private readonly IRestClient _http;
    public RestFolderApi(IRestClient http) => _http = http;

    public Task<FolderInfo?> GetFolderByQuery(string folderQuery, CancellationToken ct)
    {
        // var dto = await _http.GetAsync<FolderDto>($"folders?name={folderName}", ct);
        // return new FolderInfo(dto.Id, dto.Name);
        return Task.FromResult<FolderInfo?>(null);
    }

    public Task<FolderInfo> GetByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

}
