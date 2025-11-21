// SyllabusPlusPanopto.Infrastructure/Telemetry/AppInsightsIntegrationTelemetry.cs

using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Telemetry
{
    /// <summary>
    /// Application Insights-backed telemetry sink.
    /// Wraps everything as custom events with consistent property names.
    /// </summary>
    public sealed class AppInsightsIntegrationTelemetry : IIntegrationTelemetry
    {
        private readonly TelemetryClient _client;

        public AppInsightsIntegrationTelemetry(TelemetryClient client)
        {
            _client = client;
        }

        public string BeginRun(string sourceName, DateTime utcNow)
        {
            var runId = Guid.NewGuid().ToString("n");

            var ev = new EventTelemetry("PanoptoSync.RunStarted");
            ev.Properties["runId"] = runId;
            ev.Properties["source"] = sourceName;
            ev.Properties["startedAtUtc"] = utcNow.ToString("O");
            _client.TrackEvent(ev);

            return runId;
        }

        public void TrackSourceEvent(string runId, SourceEvent sourceEvent)
        {
            var ev = new EventTelemetry("PanoptoSync.SourceEvent");
            ev.Properties["runId"] = runId;
            ev.Properties["moduleCode"] = sourceEvent.ModuleCode ?? string.Empty;
            ev.Properties["moduleCrn"] = sourceEvent.ModuleCRN ?? string.Empty;
            ev.Properties["startDate"] = sourceEvent.StartDate.ToString("yyyy-MM-dd");
            ev.Properties["location"] = sourceEvent.LocationName ?? string.Empty;
            _client.TrackEvent(ev);
        }

        public void TrackSuccess(string runId, ScheduledSession session)
        {
            var ev = new EventTelemetry("PanoptoSync.ItemSucceeded");
            ev.Properties["runId"] = runId;
            ev.Properties["title"] = session.Title ?? string.Empty;
            ev.Properties["folder"] = session.FolderName ?? string.Empty;
            ev.Properties["recorder"] = session.RecorderName ?? string.Empty;
            ev.Properties["owner"] = session.Owner ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(session.Hash))
                ev.Properties["hash"] = session.Hash;
            _client.TrackEvent(ev);
        }

        public void TrackFailure(string runId, SourceEvent sourceEvent, Exception ex)
        {
            var ev = new ExceptionTelemetry(ex)
            {
                Message = "PanoptoSync.ItemFailed"
            };
            ev.Properties["runId"] = runId;
            ev.Properties["moduleCode"] = sourceEvent.ModuleCode ?? string.Empty;
            ev.Properties["location"] = sourceEvent.LocationName ?? string.Empty;
            _client.TrackException(ev);
        }

        public void EndRun(string runId, int total, int success, int failed)
        {
            var ev = new EventTelemetry("PanoptoSync.RunCompleted");
            ev.Properties["runId"] = runId;
            ev.Metrics["total"] = total;
            ev.Metrics["success"] = success;
            ev.Metrics["failed"] = failed;
            _client.TrackEvent(ev);
        }
    }
}
