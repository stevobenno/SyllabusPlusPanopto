namespace SyllabusPlusPanopto.Integration.Domain.Settings;


public sealed  class PanoptoBindingOptions
{
    public int SendTimeoutSeconds { get; set; } = 60;
    public int OpenTimeoutSeconds { get; set; } = 60;
    public int ReceiveTimeoutMinutes { get; set; } = 30;
    public int CloseTimeoutSeconds { get; set; } = 60;
}
