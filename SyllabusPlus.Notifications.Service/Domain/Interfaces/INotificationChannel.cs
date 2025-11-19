using SyllabusPlus.Notifications.Service.Domain.Events;

namespace SyllabusPlus.Notifications.Service.Domain.Interfaces;

public interface INotificationChannel
{
    // Logical channel name, for example "Email", "Teams"
    string ChannelName { get; }

    Task SendAsync(
        string subject,
        string body,
        NotificationEvent notification,
        CancellationToken ct = default);
}
