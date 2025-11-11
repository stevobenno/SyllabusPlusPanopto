using System;
using System.Collections.Generic;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.Telemetry
{
    /// <summary>
    /// Fans out telemetry calls to one or more underlying sinks
    /// (AppInsights, Teams, file, whatever).
    /// </summary>
    public sealed class CompositeIntegrationTelemetry : IIntegrationTelemetry
    {
        private readonly IReadOnlyList<IIntegrationTelemetry> _sinks;

        public CompositeIntegrationTelemetry(IEnumerable<IIntegrationTelemetry> sinks)
        {
            _sinks = new List<IIntegrationTelemetry>(sinks);
        }

        public string BeginRun(string sourceName, DateTime utcNow)
        {
            // we need a single runId – we generate one here and pass it down
            var runId = Guid.NewGuid().ToString("n");
            foreach (var sink in _sinks)
            {
                sink.BeginRun(sourceName, utcNow);
            }
            return runId;
        }

        public void TrackSourceEvent(string runId, SourceEvent sourceEvent)
        {
            foreach (var sink in _sinks)
            {
                sink.TrackSourceEvent(runId, sourceEvent);
            }
        }

        public void TrackSuccess(string runId, ScheduledSession session)
        {
            foreach (var sink in _sinks)
            {
                sink.TrackSuccess(runId, session);
            }
        }

        public void TrackFailure(string runId, SourceEvent sourceEvent, Exception ex)
        {
            foreach (var sink in _sinks)
            {
                sink.TrackFailure(runId, sourceEvent, ex);
            }
        }

        public void EndRun(string runId, int total, int success, int failed)
        {
            foreach (var sink in _sinks)
            {
                sink.EndRun(runId, total, success, failed);
            }
        }
    }
}
