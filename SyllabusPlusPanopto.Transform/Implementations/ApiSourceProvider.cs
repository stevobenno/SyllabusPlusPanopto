using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Implementations
{
    public sealed class ApiSourceProvider : ISourceDataProvider
    {
        private readonly HttpClient _http;

        public ApiSourceProvider(HttpClient http)
        {
            _http = http;
        }

        public async IAsyncEnumerable<SourceEvent> ReadAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // Single-shot call for now. Extend to paging later if needed.
            var rows = await _http.GetFromJsonAsync<List<SourceEvent>>("classes", ct);

            if (rows is null)
                yield break;

            foreach (var r in rows)
            {
                if (ct.IsCancellationRequested) yield break;
                yield return r;
            }
        }
    }
}
