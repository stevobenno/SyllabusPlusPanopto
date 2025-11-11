// SyllabusPlusPanopto.Infrastructure/Telemetry/IIntegrationTelemetry.cs

using System;
using SyllabusPlusPanopto.Transform.Domain;

namespace SyllabusPlusPanopto.Transform.Interfaces
{
    /// <summary>
    /// Abstraction over Application Insights so we can:
    /// - unit test without AI
    /// - keep all customDimensions consistent
    /// - later add dependency/perf tracking without touching business code
    /// </summary>
    public interface IIntegrationTelemetry
    {
        /// <summary>
        /// Called once per run, right at the start.
        /// </summary>
        string BeginRun(string sourceName, DateTime utcNow);

        /// <summary>
        /// Called for every source event we attempt to process.
        /// </summary>
        void TrackSourceEvent(string runId, SourceEvent sourceEvent);

        /// <summary>
        /// Called for every successfully synced session.
        /// </summary>
        void TrackSuccess(string runId, ScheduledSession session);

        /// <summary>
        /// Called when an item fails.
        /// </summary>
        void TrackFailure(string runId, SourceEvent sourceEvent, Exception ex);

        /// <summary>
        /// Called once per run at the end.
        /// </summary>
        void EndRun(string runId, int total, int success, int failed);
    }
}
