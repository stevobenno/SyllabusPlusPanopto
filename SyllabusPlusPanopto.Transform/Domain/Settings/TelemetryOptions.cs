namespace SyllabusPlusPanopto.Integration.Domain.Settings;

public sealed class TelemetryOptions
{
    public bool UseAppInsights { get; set; }
    public bool UseTeams { get; set; }
    public string TeamsWebhookUrl { get; set; }
}
