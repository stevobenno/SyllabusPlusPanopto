using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort;

internal sealed class RestRecorderApi : IRecorderApi
{
    private readonly IRestClient _http;
    public RestRecorderApi(IRestClient http) => _http = http;

    public Task<RecorderInfo?> GetByNameAsync(string recorderName, CancellationToken ct)
    {
        // Example future call:
        // var dto = await _http.GetAsync<RecorderDto>($"recorders?name={Uri.EscapeDataString(recorderName)}", ct);
        // return new RecorderInfo(dto.Id, dto.Name);
        return Task.FromResult<RecorderInfo?>(null);
    }
}
