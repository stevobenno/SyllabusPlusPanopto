using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Telemetry
{
    public sealed class NoopIntegrationTelemetry : IIntegrationTelemetry
    {
        public string BeginRun(string sourceName, System.DateTime utcNow) => "noop";
        public void TrackSourceEvent(string runId, SourceEvent sourceEvent) { }
        public void TrackSuccess(string runId, ScheduledSession session) { }
        public void TrackFailure(string runId, SourceEvent sourceEvent, System.Exception ex) { }
        public void EndRun(string runId, int total, int success, int failed) { }
    }
}
