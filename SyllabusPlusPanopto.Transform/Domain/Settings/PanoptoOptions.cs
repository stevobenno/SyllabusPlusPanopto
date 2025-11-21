namespace SyllabusPlusPanopto.Transform.Domain.Settings;

public sealed class PanoptoOptions
{
    public string BaseUrl { get; set; }

    public string RemoteRecorderManagementPath { get; set; }
    public string SessionManagementPath { get; set; }
    public string UserManagementPath { get; set; }
    public string UsageReportingPath { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }

    public string DateTimeFormat { get; set; } = "dd-MMM-yyyy hh:mm tt";

    public PanoptoNotifyOptions Notify { get; set; } = new();
    public PanoptoBindingOptions Binding { get; set; } = new();
}
