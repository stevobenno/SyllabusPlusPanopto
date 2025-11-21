using System.Collections.Generic;
using SyllabusPlusPanopto.Integration.Domain;

namespace SyllabusPlusPanopto.Integration.Interfaces
{
    /// <summary>
    /// Defines a transformer that converts raw Syllabus Plus / Argos source events
    /// into the canonical Panopto scheduling session model.
    ///
    /// The transformation logic follows the mapping rules documented in:
    ///   • "MEL Schedule Maker - Working.xlsx"
    ///   • "Panopto Integration Property Matrix"
    ///
    /// Consistency across CSV, SQL View, and API sources is essential — this service
    /// is the single point of truth for how an upstream event becomes a Panopto session.
    /// </summary>
    public interface ITransformService
    {
        /// <summary>
        /// Transforms a single source event into a Panopto-ready scheduled session.
        /// Intended for streaming or per-item orchestration scenarios (e.g. ProcessFlow).
        /// </summary>
        /// <param name="sourceEvent">Raw event record from the Argos/S+ data source.</param>
        /// <returns>A fully populated <see cref="ScheduledSession"/> object.</returns>
        ScheduledSession Transform(SourceEvent sourceEvent);

        /// <summary>
        /// Transforms multiple source events in one batch.
        /// Useful for testing or when the provider materialises the dataset.
        /// </summary>
        /// <param name="sourceEvents">Enumerable of raw S+ events.</param>
        /// <returns>A read-only list of transformed <see cref="ScheduledSession"/> objects.</returns>
        IReadOnlyList<ScheduledSession> Transform(IEnumerable<SourceEvent> sourceEvents);
    }
}
