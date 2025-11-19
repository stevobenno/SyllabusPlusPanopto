using Microsoft.Extensions.Options;
using SyllabusPlus.Notifications.Service.Domain.Events;
using SyllabusPlus.Notifications.Service.Domain.Interfaces;
using SyllabusPlus.Notifications.Service.Domain.Models;

namespace SyllabusPlus.Notifications.Service.Application.Processing
{
    public sealed class NotificationProcessor : INotificationProcessor
    {
        private readonly ITemplateRenderer _renderer;
        private readonly INotificationChannelRegistry _channelRegistry;
        private readonly NotificationTemplateOptions _templateOptions;

        public NotificationProcessor(
            ITemplateRenderer renderer,
            INotificationChannelRegistry channelRegistry,
            IOptions<NotificationTemplateOptions> templateOptions)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _channelRegistry = channelRegistry ?? throw new ArgumentNullException(nameof(channelRegistry));
            _templateOptions = templateOptions?.Value
                               ?? throw new ArgumentNullException(nameof(templateOptions));
        }

        public async Task ProcessAsync(NotificationEvent notification, CancellationToken ct = default)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var key = $"{notification.EventType}:{notification.Channel}";

            if (!_templateOptions.Templates.TryGetValue(key, out var templateConfig))
                throw new InvalidOperationException($"No template configured for '{key}'.");

            var model = new { notification.Data };

            var body = await _renderer.RenderAsync(templateConfig.TemplatePath, model, ct);
            var subject = SubstituteSubject(templateConfig.Subject, notification.Data);

            var channel = _channelRegistry.GetChannel(notification.Channel);
            await channel.SendAsync(subject, body, notification, ct);
        }

        private static string SubstituteSubject(string subject, IDictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(subject) || data == null) return subject;

            var result = subject;
            foreach (var kvp in data)
            {
                var token = "{{" + kvp.Key + "}}";
                var value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(token, value, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }
    }
}
