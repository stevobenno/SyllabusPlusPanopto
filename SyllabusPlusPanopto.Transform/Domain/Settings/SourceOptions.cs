namespace SyllabusPlusPanopto.Integration.Domain.Settings;

public sealed class SourceOptions
{
    public SourceKind Kind { get; set; }      // "Csv" / "Sql" / "Api"
    public string CsvPath { get; set; }
    public string SqlConnectionString { get; set; }
    public string SqlViewName { get; set; }
    public string ApiBaseUrl { get; set; }
    public string ApiKey { get; set; }
}
