using SyllabusPlusPanopto.Transform.To_Sort;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

public interface ISessionApi
{
    Task<ScheduleResult> ScheduleAsync(
        string title,
        Guid folderId,
        bool webcast,
        DateTime startUtc,
        DateTime endUtc,
        IEnumerable<Guid> recorderIds,
        bool overwrite,
        CancellationToken ct);

    Task SetOwnerAsync(Guid sessionId, string owner, CancellationToken ct);
    Task SetExternalIdAsync(Guid sessionId, string externalId, CancellationToken ct);
    Task SetAvailabilityStartAsync(Guid sessionId, DateTime startUtc, CancellationToken ct);

    Task DeleteAsync(IEnumerable<Guid> sessionIds, CancellationToken ct);

    /// <summary>List existing scheduled sessions in a time window, including ExternalId.</summary>
    Task<IReadOnlyList<ExistingSession>> ListScheduledAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}
