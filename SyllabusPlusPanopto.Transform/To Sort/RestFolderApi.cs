using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort;

internal sealed class RestFolderApi : IFolderApi
{
    private readonly IRestClient _http;
    public RestFolderApi(IRestClient http) => _http = http;

    public Task<FolderInfo?> GetByNameAsync(string folderName, CancellationToken ct)
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
