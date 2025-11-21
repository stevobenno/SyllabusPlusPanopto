using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Integration.Interfaces
{
    /// <summary>
    /// Run-scoped working set. No persistence; seeded from Panopto at BeginRun.
    /// Uses ExternalId (our hash) as the key.
    /// </summary>
    public interface IWorkingStore
    {
        Task BeginRunAsync(string runId, DateTime startedUtc, CancellationToken ct);
        Task CompleteRunAsync(string runId, int read, int upserts, int unchanged, int errors, int deleted, CancellationToken ct);

        Task<string> GetHashAsync(string externalId, CancellationToken ct);
        Task UpsertHashAsync(string externalId, string hash, DateTime utcNow, CancellationToken ct);

        Task MarkSeenAsync(string runId, string externalId, string hash, CancellationToken ct);

        Task<string[]> GetKeysNotSeenThisRunAsync(string runId, int horizonDays, DateTime utcNow, CancellationToken ct);

        /// <summary>Map ExternalIds to Panopto sessionIds gathered at BeginRun.</summary>
        Task<Guid[]> GetSessionIdsByExternalIdsAsync(IEnumerable<string> externalIds, CancellationToken ct);
    }
}
