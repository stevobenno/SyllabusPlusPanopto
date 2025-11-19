namespace SyllabusPlus.Notifications.Service.Domain.Interfaces;

public interface ITemplateRenderer
{
    Task<string> RenderAsync(string templatePath, object model, CancellationToken ct = default);
}
