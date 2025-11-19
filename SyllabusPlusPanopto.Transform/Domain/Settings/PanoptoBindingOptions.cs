namespace SyllabusPlusPanopto.Transform.Domain.Settings;

public class PanoptoBindingOptions
{
    public int SendTimeoutSeconds { get; set; } = 60;
    public int OpenTimeoutSeconds { get; set; } = 60;
    public int ReceiveTimeoutMinutes { get; set; } = 30;
    public int CloseTimeoutSeconds { get; set; } = 60;
    public long MaxReceivedMessageSize { get; set; } = 20_000_000;
    public int MaxStringContentLength { get; set; } = 20_000_000;
    public int MaxArrayLength { get; set; } = 20_000_000;
}