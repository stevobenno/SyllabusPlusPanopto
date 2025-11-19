namespace SyllabusPlus.Notifications.Service.Domain.Interfaces;

public interface INotificationChannelRegistry
{
    INotificationChannel GetChannel(string channelName);
}
