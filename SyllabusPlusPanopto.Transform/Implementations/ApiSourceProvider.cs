using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SyllabusPlusPanopto.Integration.Implementations
{
    public sealed class ApiSourceProvider : ISourceDataProvider
    {
        private readonly HttpClient _http;
        private readonly SourceOptions _options;

        public ApiSourceProvider(HttpClient http, IOptions<SourceOptions> opts)
        {
            _http = http;
            _options = opts.Value;

            if (string.IsNullOrWhiteSpace(_options.ApiBaseUrl))
                throw new InvalidOperationException("Source.ApiBaseUrl must be set for API mode.");

            _http.BaseAddress = new Uri(_options.ApiBaseUrl);
        }

        public async IAsyncEnumerable<SourceEvent> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            var rows = await _http.GetFromJsonAsync<List<SourceEvent>>("classes", ct)
                       ?? new List<SourceEvent>();

            foreach (var r in rows)
                yield return r;
        }
    }
}
