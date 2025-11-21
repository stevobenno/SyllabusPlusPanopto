using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public class RestSessionApi : ISessionApi
{
    public RestSessionApi(IRestClient client)
    {
        throw new System.NotImplementedException();
    }

    public Task<ScheduleResult> ScheduleAsync(string title, Guid folderId, bool webcast, DateTime startUtc, DateTime endUtc, IEnumerable<Guid> recorderIds, bool overwrite, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetOwnerAsync(Guid sessionId, string owner, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetExternalIdAsync(Guid sessionId, string externalId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task SetAvailabilityStartAsync(Guid sessionId, DateTime startUtc, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(IEnumerable<Guid> sessionIds, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ExistingSession>> ListScheduledAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
