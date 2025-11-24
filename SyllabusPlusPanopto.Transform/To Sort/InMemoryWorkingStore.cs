using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    /// <summary>
    /// Run-scoped, in-memory working set seeded from Panopto at BeginRun.
    /// No persistence; satisfies "no transient storage" constraint.
    /// 
    /// Responsibilities:
    ///   • Track Syllabus+ managed sessions by ExternalId
    ///   • Track "alien" sessions (no ExternalId) for pre-sync deletion
    ///   • Provide simple hash existence checks and deletion candidates
    /// </summary>
    public sealed class InMemoryWorkingStore : IWorkingStore
    {
        private readonly ISessionApi _sessions;

        // externalId -> (sessionId, lastSeenUtc)
        private readonly Dictionary<string, (Guid sessionId, DateTime lastSeenUtc)> _existing
            = new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _seenThisRun
            = new(StringComparer.OrdinalIgnoreCase);

        // Panopto sessions that had *no* ExternalId at the start of the run
        // (manual, legacy, or otherwise not Syllabus+ owned)
        private readonly List<Guid> _alienSessionIds = new();

        private string _runId = string.Empty;

        public InMemoryWorkingStore(ISessionApi sessions)
        {
            _sessions = sessions;
        }

        public async Task BeginRunAsync(string runId, DateTime startedUtc, CancellationToken ct)
        {
            _runId = runId;
            _seenThisRun.Clear();
            _existing.Clear();
            _alienSessionIds.Clear();

            // Note: seeding happens in SyncService via ListScheduledAsync to control the window.
            // This implementation expects SyncService to call SeedExisting(...) before first SyncAsync.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called by the sync service once after ListScheduledAsync.
        /// 
        /// Seeds:
        ///   • Syllabus+ sessions (those with ExternalId)
        ///   • "Alien" sessions (no ExternalId) into a separate list so they can be
        ///     deleted before reconciliation.
        /// 
        /// Outputs:
        ///   seededCount = number of S+ managed sessions
        ///   alienCount  = number of alien/manual/legacy sessions
        /// </summary>
        public void SeedExisting(
            IEnumerable<ExistingSession> existing,
            out int seededCount,
            out int alienCount)
        {
            _existing.Clear();
            _alienSessionIds.Clear();

            var now = DateTime.UtcNow;
            seededCount = 0;
            alienCount = 0;

            foreach (var e in existing)
            {
                if (string.IsNullOrWhiteSpace(e.ExternalId))
                {
                    // Alien session: exists in Panopto but not tagged as one of ours
                    _alienSessionIds.Add(e.SessionId);
                    alienCount++;
                }
                else
                {
                    // Normal S+ managed session keyed by ExternalId
                    _existing[e.ExternalId] = (e.SessionId, now);
                    seededCount++;
                }
            }
        }

        public Task CompleteRunAsync(
            string runId,
            int read,
            int upserts,
            int unchanged,
            int errors,
            int deleted,
            CancellationToken ct)
        {
            // Nothing to persist; this is purely in-memory.
            return Task.CompletedTask;
        }

        public Task<string?> GetHashAsync(string externalId, CancellationToken ct)
        {
            // If Panopto already has a session with this ExternalId, pretend we "know" the hash.
            // The hash we store is simply the ExternalId itself.
            var exists = _existing.ContainsKey(externalId);
            return Task.FromResult<string?>(exists ? externalId : null);
        }

        public Task UpsertHashAsync(string externalId, string hash, DateTime utcNow, CancellationToken ct)
        {
            // For in-memory, just ensure key exists.
            // When we create a new session this run, we don't yet know the Panopto sessionId,
            // so we keep Guid.Empty as a placeholder.
            if (!_existing.ContainsKey(externalId))
            {
                _existing[externalId] = (Guid.Empty, utcNow);
            }
            else
            {
                // Update lastSeenUtc for existing entries
                var current = _existing[externalId];
                _existing[externalId] = (current.sessionId, utcNow);
            }

            return Task.CompletedTask;
        }

        public Task MarkSeenAsync(string runId, string externalId, string hash, CancellationToken ct)
        {
            _seenThisRun.Add(externalId);
            return Task.CompletedTask;
        }

        public Task<string[]> GetKeysNotSeenThisRunAsync(
            string runId,
            int horizonDays,
            DateTime utcNow,
            CancellationToken ct)
        {
            // Deletions: everything that existed at start (in _existing) but not seen this run.
            // Horizon can be applied by the caller using the time window they enumerated.
            var toDelete = _existing.Keys
                .Where(k => !_seenThisRun.Contains(k))
                .ToArray();

            return Task.FromResult(toDelete);
        }

        public Task<Guid[]> GetSessionIdsByExternalIdsAsync(
            IEnumerable<string> externalIds,
            CancellationToken ct)
        {
            var ids = externalIds
                .Select(k => _existing.TryGetValue(k, out var v) ? v.sessionId : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToArray();

            return Task.FromResult(ids);
        }

        /// <summary>
        /// Returns the Panopto session IDs that had no ExternalId
        /// when we seeded the store (alien/manual/legacy sessions).
        /// 
        /// Used by the sync service to perform a pre-sync purge.
        /// </summary>
        public Guid[] GetAlienSessionIds()
            => _alienSessionIds.ToArray();
    }
}
