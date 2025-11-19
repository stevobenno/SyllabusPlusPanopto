namespace SyllabusPlus.Notifications.Service.Domain.Models;

public sealed class TemplateConfig
{
    // Path relative to content root, for example "Templates/RecordingProcessedEmail.html"
    public string TemplatePath { get; set; }
    public string Subject { get; set; }
}
