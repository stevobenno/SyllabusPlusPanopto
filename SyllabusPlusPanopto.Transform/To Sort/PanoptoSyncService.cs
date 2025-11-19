using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort
{
    public sealed class PanoptoSyncService : ISyncService
    {
        private readonly IPanoptoPlatform _platform;
        private readonly IWorkingStore _store;
        private readonly ILogger<PanoptoSyncService> _log;

        private SyncRunContext _ctx = default!;
        private int _read, _upserts, _unchanged, _errors;

        public PanoptoSyncService(IPanoptoPlatform platform, IWorkingStore store, ILogger<PanoptoSyncService> log)
        {
            _platform = platform;
            _store = store;
            _log = log;
        }

        public async Task BeginRunAsync(SyncRunContext ctx, CancellationToken ct = default)
        {
            _ctx = ctx;
            _read = _upserts = _unchanged = _errors = 0;

            // 1) Start run
            await _store.BeginRunAsync(ctx.RunId, ctx.UtcNow, ct);

            // 2) Enumerate existing sessions in scope and seed the in-memory store
            var existing = await _platform.Sessions.ListScheduledAsync(ctx.ListFromUtc, ctx.ListToUtc, ct);
            if (_store is InMemoryWorkingStore mem)
                mem.SeedExisting(existing);

            _log.LogInformation("Sync run {RunId} started. Seeded {Count} existing sessions from Panopto.", ctx.RunId, existing.Count);
        }

        public async Task SyncAsync(ScheduledSession s, CancellationToken ct = default)
        {
            _read++;

            try
            {
                var externalId = ComputeHash(s);
                s.Hash = externalId;

                // mark seen early
                await _store.MarkSeenAsync(_ctx.RunId, externalId, externalId, ct);

                // If a session with this ExternalId already exists in Panopto, treat as unchanged/updatable
                var already = await _store.GetHashAsync(externalId, ct);
                if (already != null)
                {
                    _unchanged++;

                    if (!_ctx.DryRun)
                    {
                        // Optionally refresh owner/externalId/availability if desired:
                        // We need the sessionId to do that; fetch from store mapping.
                        var ids = await _store.GetSessionIdsByExternalIdsAsync(new[] { externalId }, ct);
                        if (ids.Length == 1)
                        {
                            var id = ids[0];
                            await _platform.Sessions.SetExternalIdAsync(id, externalId, ct);
                            if (!string.IsNullOrWhiteSpace(s.Owner))
                                await _platform.Sessions.SetOwnerAsync(id, s.Owner, ct);
                            await _platform.Sessions.SetAvailabilityStartAsync(id, s.StartTimeUtc, ct);
                        }
                    }

                    return;
                }

                if (_ctx.DryRun)
                {
                    _log.LogInformation("DRYRUN Create {Title} [{ExtId}]", s.Title, externalId);
                    await _store.UpsertHashAsync(externalId, externalId, _ctx.UtcNow, ct);
                    _upserts++;
                    return;
                }

                // Resolve folder
                var folder = TryParseGuid(s.FolderName, out var fid)
                    ? await _platform.Folders.GetByIdAsync(fid, ct)
                    : await _platform.Folders.GetByNameAsync(s.FolderName, ct);

                if (folder is null)
                    throw new InvalidOperationException($"Folder not found for '{s.FolderName}' (Title '{s.Title}')");

                // Resolve recorder
                var rec = await _platform.Recorders.GetByNameAsync(s.RecorderName, ct);
                if (rec is null)
                    throw new InvalidOperationException($"Recorder not found: '{s.RecorderName}'");

                // Schedule
                var result = await _platform.Sessions.ScheduleAsync(
                    title: s.Title,
                    folderId: folder.Id,
                    webcast: s.Webcast == 1,
                    startUtc: s.StartTimeUtc,
                    endUtc: s.EndTimeUtc,
                    recorderIds: new[] { rec.Id },
                    overwrite: true,
                    ct);

                if (!result.Success || result.SessionId is null)
                    throw new InvalidOperationException($"Schedule failed: {result.LogLine}");

                var sessionId = result.SessionId.Value;

                // Metadata
                await _platform.Sessions.SetExternalIdAsync(sessionId, externalId, ct);
                if (!string.IsNullOrWhiteSpace(s.Owner))
                    await _platform.Sessions.SetOwnerAsync(sessionId, s.Owner, ct);
                await _platform.Sessions.SetAvailabilityStartAsync(sessionId, s.StartTimeUtc, ct);

                await _store.UpsertHashAsync(externalId, externalId, _ctx.UtcNow, ct);
                _upserts++;
            }
            catch (Exception ex)
            {
                _errors++;
                _log.LogError(ex, "Sync failed for {Title}", s?.Title ?? "(null)");
            }
        }

        public async Task CompleteRunAsync(CancellationToken ct = default)
        {
            var allowDeletes = _ctx.AllowDeletions;
            if (_ctx.MinExpectedRows.HasValue && _read < _ctx.MinExpectedRows.Value)
            {
                allowDeletes = false;
                _log.LogWarning("Low-water guard: Read={Read} Min={Min}. Skipping deletions.", _read, _ctx.MinExpectedRows);
            }

            var toDeleteExternalIds = allowDeletes
                ? await _store.GetKeysNotSeenThisRunAsync(_ctx.RunId, _ctx.DeleteHorizonDays, _ctx.UtcNow, ct)
                : Array.Empty<string>();

            var deleted = 0;
            if (toDeleteExternalIds.Length > 0)
            {
                if (_ctx.DryRun)
                {
                    _log.LogInformation("DRYRUN would delete {Count} session(s).", toDeleteExternalIds.Length);
                }
                else
                {
                    var ids = await _store.GetSessionIdsByExternalIdsAsync(toDeleteExternalIds, ct);
                    if (ids.Length > 0)
                    {
                        await _platform.Sessions.DeleteAsync(ids, ct);
                        deleted = ids.Length;
                    }
                }
            }

            await _store.CompleteRunAsync(_ctx.RunId, _read, _upserts, _unchanged, _errors, deleted, ct);
            _log.LogInformation("Run {RunId} complete Read={R} Upserts={U} Unchanged={N} Errors={E} Deleted={D} DryRun={Dry}",
                _ctx.RunId, _read, _upserts, _unchanged, _errors, deleted, _ctx.DryRun);
        }

        private static bool TryParseGuid(string value, out Guid id) => Guid.TryParse(value, out id);

        private static string ComputeHash(ScheduledSession s)
        {
            var sb = new StringBuilder(256);
            sb.AppendLine(s.Title);
            sb.AppendLine(s.StartTimeUtc.ToString("O"));
            sb.AppendLine(s.EndTimeUtc.ToString("O"));
            sb.AppendLine(s.FolderName);
            sb.AppendLine(s.RecorderName);
            sb.AppendLine(s.Webcast == 1 ? "1" : "0");
            sb.AppendLine(s.Owner);
            sb.AppendLine(s.Description);

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(bytes);
        }
    }
}
