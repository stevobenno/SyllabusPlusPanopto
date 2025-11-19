using SyllabusPlusPanopto.Transform.Interfaces;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Transform.To_Sort
{
    /// <summary>
    /// Run-scoped, in-memory working set seeded from Panopto at BeginRun.
    /// No persistence; satisfies "no transient storage" constraint.
    /// </summary>
    public sealed class InMemoryWorkingStore : IWorkingStore
    {
        private readonly ISessionApi _sessions;

        // externalId -> (sessionId, lastSeenUtc)
        private readonly Dictionary<string, (Guid sessionId, DateTime lastSeenUtc)> _existing = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _seenThisRun = new(StringComparer.OrdinalIgnoreCase);

        private string _runId = string.Empty;

        public InMemoryWorkingStore(ISessionApi sessions) => _sessions = sessions;

        public async Task BeginRunAsync(string runId, DateTime startedUtc, CancellationToken ct)
        {
            _runId = runId;
            _seenThisRun.Clear();
            _existing.Clear();
            // Note: seeding happens in SyncService via ListScheduledAsync to control the window.
            // This implementation expects SyncService to call SeedExistingAsync before first SyncAsync.
            await Task.CompletedTask;
        }

        /// <summary>Called by the sync service once after ListScheduledAsync.</summary>
        public void SeedExisting(IEnumerable<ExistingSession> existing)
        {
            _existing.Clear();
            foreach (var e in existing)
            {
                if (string.IsNullOrWhiteSpace(e.ExternalId)) continue;
                _existing[e.ExternalId] = (e.SessionId, DateTime.UtcNow);
            }
        }

        public Task CompleteRunAsync(string runId, int read, int upserts, int unchanged, int errors, int deleted, CancellationToken ct)
            => Task.CompletedTask;

        public Task<string?> GetHashAsync(string externalId, CancellationToken ct)
        {
            // If Panopto already has a session with this ExternalId, pretend we "know" the hash
            var exists = _existing.ContainsKey(externalId);
            return Task.FromResult<string?>(exists ? externalId : null);
        }

        public Task UpsertHashAsync(string externalId, string hash, DateTime utcNow, CancellationToken ct)
        {
            // For in-memory, just ensure key exists (no mapping unless we created it this run).
            if (!_existing.ContainsKey(externalId))
                _existing[externalId] = (Guid.Empty, utcNow);
            return Task.CompletedTask;
        }

        public Task MarkSeenAsync(string runId, string externalId, string hash, CancellationToken ct)
        {
            _seenThisRun.Add(externalId);
            return Task.CompletedTask;
        }

        public Task<string[]> GetKeysNotSeenThisRunAsync(string runId, int horizonDays, DateTime utcNow, CancellationToken ct)
        {
            // Deletions: everything that existed at start (in _existing) but not seen this run.
            // Horizon can be applied by the caller using the time window they enumerated.
            var toDelete = _existing.Keys.Where(k => !_seenThisRun.Contains(k)).ToArray();
            return Task.FromResult(toDelete);
        }

        public Task<Guid[]> GetSessionIdsByExternalIdsAsync(IEnumerable<string> externalIds, CancellationToken ct)
        {
            var ids = externalIds
                .Select(k => _existing.TryGetValue(k, out var v) ? v.sessionId : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToArray();
            return Task.FromResult(ids);
        }
    }
}
