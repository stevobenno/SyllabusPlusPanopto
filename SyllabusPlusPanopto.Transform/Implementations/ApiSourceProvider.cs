using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.Implementations;

public sealed class ApiSourceProvider : ISourceDataProvider
{
    private readonly HttpClient _http;
    public ApiSourceProvider(HttpClient http) => _http = http;

    public async IAsyncEnumerable<SourceEvent> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        // simple single-shot; extend to paging if needed
        var rows = await _http.GetFromJsonAsync<List<SourceEvent>>("classes", ct) ;
        foreach (var r in rows) yield return r;
    }
}
