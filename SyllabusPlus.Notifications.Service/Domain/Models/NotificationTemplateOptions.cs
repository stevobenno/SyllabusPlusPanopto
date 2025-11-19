namespace SyllabusPlus.Notifications.Service.Domain.Models;

public sealed class NotificationTemplateOptions
{
    // Key is "EventType:Channel", for example "RecordingProcessed:Email"
    public Dictionary<string, TemplateConfig> Templates { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
