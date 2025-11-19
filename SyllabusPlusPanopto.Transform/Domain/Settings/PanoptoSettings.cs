using SyllabusPlusPanopto.Transform.Domain.Settings;

public class PanoptoSettings
{
    public string BaseUrl { get; set; }

    public string RemoteRecorderManagementPath { get; set; }
    public string SessionManagementPath { get; set; }
    public string UserManagementPath { get; set; }
    public string UsageReportingPath { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }

    public string DateTimeFormat { get; set; } = "dd-MMM-yyyy hh:mm tt";

    public NotifyOptions Notify { get; set; } = new();
    public PanoptoBindingOptions Binding { get; set; } = new();
}
