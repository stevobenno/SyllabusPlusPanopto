using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Telemetry
{
    /// <summary>
    /// Sends high-level integration events to a Teams channel via an incoming webhook.
    /// This is deliberately lightweight – we don’t spam every row, only the important bits.
    /// </summary>
    public sealed class TeamsWebhookIntegrationTelemetry : IIntegrationTelemetry
    {
        private readonly HttpClient _http;
        private readonly string _webhookUrl;

        public TeamsWebhookIntegrationTelemetry(HttpClient httpClient, string webhookUrl)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        }

        public string BeginRun(string sourceName, DateTime utcNow)
        {
            // we could send a message here, but Teams channels get noisy.
            // So just return a runId.
            return Guid.NewGuid().ToString("n");
        }

        public void TrackSourceEvent(string runId, SourceEvent sourceEvent)
        {
            // skip per-row to avoid spamming Teams
        }

        public void TrackSuccess(string runId, ScheduledSession session)
        {
            // skip per-row
        }

        public void TrackFailure(string runId, SourceEvent sourceEvent, Exception ex)
        {
            // failures are worth posting
            var text =
                $"**Panopto sync failed**\nRun: `{runId}`\nModule: `{sourceEvent.ModuleCode}`\nRoom: `{sourceEvent.LocationName}`\nError: `{ex.Message}`";

            PostCard(text);
        }

        public void EndRun(string runId, int total, int success, int failed)
        {
            var text =
                $"Panopto sync completed.\nRun: `{runId}`\nTotal: **{total}**\nSuccess: **{success}**\nFailed: **{failed}**";

            PostCard(text);
        }

        private void PostCard(string text)
        {
            var payload = new
            {
                text
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // fire-and-forget – if Teams is down we don’t break the run
            _ = _http.PostAsync(_webhookUrl, content);
        }
    }
}
