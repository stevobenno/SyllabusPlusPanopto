namespace SyllabusPlusPanopto.Transform.Domain.Settings;

public sealed class PanoptoNotifyOptions
{
    public string ExternalIdPrefix { get; set; } = "#ProcessingCompleted:";
    public string TimestampFormat { get; set; } = "yyyyMMddTHHmmssZ";
}
