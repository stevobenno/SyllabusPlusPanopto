namespace SyllabusPlus.Notifications.Service.Domain.Interfaces;

public interface IGraphEmailClient
{
    Task SendMailAsync(
        string[] recipients,
        string subject,
        string htmlBody,
        CancellationToken ct = default);
}
