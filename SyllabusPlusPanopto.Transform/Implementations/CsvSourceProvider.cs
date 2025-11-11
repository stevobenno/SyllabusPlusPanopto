using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.Implementations;

public sealed class CsvSourceProvider : ISourceDataProvider

{
    private readonly string _path;
    public CsvSourceProvider(string path) => _path = path;

    public async IAsyncEnumerable<SourceEvent> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        using var reader = File.OpenText(_path);
        string? line;
        bool headerSkipped = false;

        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (ct.IsCancellationRequested) yield break;
            if (!headerSkipped) { headerSkipped = true; continue; }

            var cols = line.Split(',');
            // Adjust indexes to your CSV
            yield return new SourceEvent(
                ModuleCode: cols[0],
                StartUtc: DateTime.Parse(cols[1], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                EndUtc: DateTime.Parse(cols[2], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                Room: cols[3],
                Recorder: cols[4],
                OwnerEmail: cols[5]);
        }
    }
}
