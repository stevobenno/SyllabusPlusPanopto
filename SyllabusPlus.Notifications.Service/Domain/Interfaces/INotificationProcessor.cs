using SyllabusPlus.Notifications.Service.Domain.Events;

namespace SyllabusPlus.Notifications.Service.Domain.Interfaces;

public interface INotificationProcessor
{
    Task ProcessAsync(NotificationEvent notification, CancellationToken ct = default);
}
