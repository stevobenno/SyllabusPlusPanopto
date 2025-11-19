using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.Implementations
{
    public sealed class CsvSourceProvider : ISourceDataProvider
    {
        private readonly string _path;

        public CsvSourceProvider(string path) => _path = path;

        public async IAsyncEnumerable<SourceEvent> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            using var reader = File.OpenText(_path);

            // Read and index the header row
            var headerLine = await reader.ReadLineAsync(ct);
            if (headerLine is null) yield break;

            var headers = headerLine.Split(',', StringSplitOptions.TrimEntries);
            var index = BuildIndex(headers);

            string? line;
            while ((line = await reader.ReadLineAsync(ct)) is not null)
            {
                if (ct.IsCancellationRequested) yield break;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',', StringSplitOptions.None);

                string Get(string name) => TryGet(cols, index, name);

                DateTime ParseDate(string name)
                {
                    var raw = Get(name);
                    return DateTime.Parse(raw, CultureInfo.InvariantCulture);
                }

                TimeSpan ParseTime(string name)
                {
                    var raw = Get(name);
                    return TimeSpan.Parse(raw, CultureInfo.InvariantCulture);
                }

                int ParseInt(string name)
                {
                    var raw = Get(name);
                    return int.TryParse(raw, out var val) ? val : 0;
                }

                yield return new SourceEvent(
                    ActivityName: Get("ActivityName"),
                    ModuleCode: Get("ModuleCode"),
                    ModuleName: Get("ModuleName"),
                    ModuleCRN: Get("ModuleCRN"),
                    StaffName: Get("StaffName"),
                    StartDate: ParseDate("StartDate"),
                    StartTime: ParseTime("StartTime"),
                    EndTime: ParseTime("EndTime"),
                    LocationName: Get("LocationName"),
                    RecorderName: Get("RecorderName"),
                    RecordingFactor: ParseInt("RecordingFactor"),
                    StaffUserName: Get("StaffUserName")
                );
            }
        }

        private static Dictionary<string, int> BuildIndex(string[] headers)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(headers[i]))
                    dict[headers[i]] = i;
            }
            return dict;
        }

        private static string TryGet(string[] cols, Dictionary<string, int> index, string name)
        {
            if (index.TryGetValue(name, out var i) && i >= 0 && i < cols.Length)
                return cols[i].Trim();
            return string.Empty;
        }
    }
}
