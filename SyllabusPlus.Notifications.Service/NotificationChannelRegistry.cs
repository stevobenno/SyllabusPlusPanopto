using SyllabusPlus.Notifications.Service.Domain.Interfaces;

namespace SyllabusPlus.Notifications.Service
{
    public sealed class NotificationChannelRegistry : INotificationChannelRegistry
    {
        private readonly Dictionary<string, INotificationChannel> _channels;

        public NotificationChannelRegistry(IEnumerable<INotificationChannel> channels)
        {
            _channels = channels.ToDictionary(
                c => c.ChannelName,
                c => c,
                StringComparer.OrdinalIgnoreCase);
        }

        public INotificationChannel GetChannel(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                throw new ArgumentNullException(nameof(channelName));

            if (!_channels.TryGetValue(channelName, out var channel))
                throw new InvalidOperationException($"No notification channel registered for name '{channelName}'.");

            return channel;
        }
    }
}
