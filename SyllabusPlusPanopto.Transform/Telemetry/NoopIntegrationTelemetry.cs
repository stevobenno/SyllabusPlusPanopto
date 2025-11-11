using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.Telemetry
{
    public sealed class NoopIntegrationTelemetry : IIntegrationTelemetry
    {
        public string BeginRun(string sourceName, System.DateTime utcNow) => "noop";
        public void TrackSourceEvent(string runId, Transform.Domain.SourceEvent sourceEvent) { }
        public void TrackSuccess(string runId, Transform.Domain.ScheduledSession session) { }
        public void TrackFailure(string runId, Transform.Domain.SourceEvent sourceEvent, System.Exception ex) { }
        public void EndRun(string runId, int total, int success, int failed) { }
    }
}
