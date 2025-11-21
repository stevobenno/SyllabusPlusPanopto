using SyllabusPlusPanopto.Integration.Domain.Settings;

namespace SyllabusPlusPanopto.Infrastructure;

/// <summary>
/// Configuration options for selecting and parameterising the source data provider.
/// Bound from the "Source" section of appsettings.json.
/// </summary>
public sealed class SourceOptions
{
    /// <summary>
    /// Which source type to use: Csv, SqlView, or Api.
    /// </summary>
    public SourceKind Kind { get; set; } = SourceKind.Unknown;

    // --- CSV options ---
    public string? CsvPath { get; set; }

    // --- SQL view options ---
    public string? SqlConnectionString { get; set; }
    public string? SqlViewName { get; set; }

    // --- API options ---
    public string? ApiBaseUrl { get; set; }
    public string? ApiKey { get; set; }
}
