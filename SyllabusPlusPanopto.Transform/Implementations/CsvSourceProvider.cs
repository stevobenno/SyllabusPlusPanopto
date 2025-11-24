using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Implementations
{
    /// <summary>
    /// ----------------------------------------------------------------------------
    ///   C S V   S O U R C E   P R O V I D E R
    /// ----------------------------------------------------------------------------
    /// This class is one of the three concrete implementations of
    /// <see cref="ISourceDataProvider"/>. It provides timetable event data by
    /// reading a CSV file from disk. Which provider is active is determined by
    /// <see cref="SourceOptions.ProviderType"/> and configured via DI.
    ///
    /// This provider is primarily intended for:
    ///     • Local development
    ///     • Test harnesses
    ///     • Scenarios where the client has not yet confirmed whether the
    ///       definitive data source will be SQL View, API, or CSV.
    ///
    /// What it does:
    ///     • Opens the configured CSV file path
    ///     • Reads the header row and builds a column index map
    ///     • Streams each subsequent row asynchronously as a <see cref="SourceEvent"/>
    ///     • Performs typed parsing for date, time and integer fields
    ///
    /// What it returns:
    ///     • A sequence of <see cref="SourceEvent"/> objects (async enumerable)
    ///     • All rows are yielded lazily (no full-file buffering)
    ///     • Empty or missing columns resolve to empty string or default values
    ///
    /// Key assumptions:
    ///     • CSV contains a header row with expected column names
    ///     • Date formats follow: dd-MM-yyyy, dd/MM/yyyy, dd.MM.yyyy
    ///     • Time fields are parseable by invariant TimeSpan.Parse
    ///
    /// Logging:
    ///     This provider logs its identity at startup so that operational logs
    ///     always make it clear which concrete provider was used.
    /// ----------------------------------------------------------------------------
    /// </summary>
    public sealed class CsvSourceProvider : ISourceDataProvider
    {
        private readonly ILogger<CsvSourceProvider> _logger;
        private readonly string _path;
        private int _rowsRead;
        private int _blankLines;
        private int _malformed;

        public CsvSourceProvider(IOptions<SourceOptions> options,ILogger<CsvSourceProvider> logger)
        {
            if (options?.Value == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.Value.CsvPath))
                throw new InvalidOperationException("Source.CsvPath must be configured for Csv source.");
            _logger = logger;

            _path = options.Value.CsvPath!;

            _logger.LogInformation(
                "\n" +
                "=============================================================\n" +
                "  S O U R C E   P R O V I D E R   A C T I V A T E D\n" +
                "=============================================================\n" +
                "  Provider Type  : CSV Source Provider\n" +
                "  File Path      : {Path}\n" +
                "  Description    : Reading timetable event data from CSV\n" +
                "=============================================================\n",
                _path
            );
        }

        public async IAsyncEnumerable<SourceEvent> ReadAsync(
      [EnumeratorCancellation] CancellationToken ct = default)
        {
            try
            {
                using var reader = File.OpenText(_path);

                var headerLine = await reader.ReadLineAsync(ct);
                if (headerLine is null)
                {
                    _logger.LogWarning("CSV Source Provider: file contains no header row.");
                    yield break;
                }

                var headers = headerLine.Split(',', StringSplitOptions.TrimEntries);
                var index = BuildIndex(headers);

                string? line;

                while ((line = await reader.ReadLineAsync(ct)) is not null)
                {
                    if (ct.IsCancellationRequested)
                        yield break;

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        _blankLines++;
                        continue;
                    }

                    var cols = line.Split(',', StringSplitOptions.None);

                    string Get(string name) => TryGet(cols, index, name);

                    DateTime ParseDate(string name) =>
                        DateTime.ParseExact(
                            Get(name),
                            new[] { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy" },
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None
                        );

                    TimeSpan ParseTime(string name) =>
                        TimeSpan.Parse(Get(name), CultureInfo.InvariantCulture);

                    int ParseInt(string name)
                    {
                        var raw = Get(name);
                        return int.TryParse(raw, out var n) ? n : 0;
                    }

                    SourceEvent? ev = null;
                    var ok = true;

                    try
                    {
                        ev = new SourceEvent(
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
                    catch
                    {
                        _malformed++;
                        ok = false;
                    }

                    if (!ok || ev is null)
                        continue;

                    _rowsRead++;
                    yield return ev;
                }
            }
            finally
            {
                _logger.LogInformation(
                    "\n" +
                    "=============================================================\n" +
                    "  C S V   R E A D   C O M P L E T E\n" +
                    "=============================================================\n" +
                    "  Provider      : CSV Source Provider\n" +
                    "  File Path     : {Path}\n" +
                    "-------------------------------------------------------------\n" +
                    "  Rows Parsed   : {Rows}\n" +
                    "  Blank Lines   : {Blank}\n" +
                    "  Malformed     : {Malformed}\n" +
                    "-------------------------------------------------------------\n" +
                    "  Completed At  : {Timestamp}\n" +
                    "=============================================================\n",
                    _path,
                    _rowsRead,
                    _blankLines,
                    _malformed,
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
                );
            }
        }


        private static Dictionary<string, int> BuildIndex(string[] headers)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
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
