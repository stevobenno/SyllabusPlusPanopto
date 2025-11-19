namespace SyllabusPlus.Notifications.Service.Domain.Events;

public sealed class NotificationEvent
{
    public string EventType { get; set; }      // "RecordingProcessed"
    public string Channel { get; set; }        // "Email", "Teams"
    public string TemplateKey { get; set; }    // "PanoptoReady"

    // Arbitrary payload. Keys like "recipients", "recordingTitle", "playbackUrl", etc.
    public IDictionary<string, object> Data { get; set; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
