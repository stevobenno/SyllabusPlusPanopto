using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public sealed class PanoptoSyncService : ISyncService
    {
        private readonly IPanoptoPlatform _platform;
        private readonly IWorkingStore _store;
        private readonly ILogger<PanoptoSyncService> _logger;
        private readonly IOptions<SyncOptions> _syncOptions;

        private SyncRunContext _ctx = default!;
        private int _read, _upserts, _unchanged, _errors;

        public PanoptoSyncService(
            IPanoptoPlatform platform,
            IWorkingStore store,
            ILogger<PanoptoSyncService> logger, IOptions<SyncOptions> syncOptions)
        {
            _platform = platform;
            _store = store;
            _logger = logger;
            _syncOptions = syncOptions;
        }

        // ========================================================================
        //  P H A S E   1 —   B E G I N   R U N
        // ========================================================================
        public async Task BeginRunAsync(SyncRunContext ctx, CancellationToken ct = default)
        {
            _ctx = ctx;
            _read = _upserts = _unchanged = _errors = 0;

            _logger.LogInformation(
                "\n" +
                "=====================================================================\n" +
                "   S Y N C   R U N   I N I T I A T I O N\n" +
                "=====================================================================\n" +
                "  Run ID           : {RunId}\n" +
                "  Dry Run          : {DryRun}\n" +
                "  Allow Deletions  : {AllowDeletions}\n" +
                "  Horizon (days)   : {Horizon}\n" +
                "  Window (UTC)     : {From}  →  {To}\n" +
                "  Started At       : {Started}\n" +
                "=====================================================================",
                ctx.RunId,
                ctx.DryRun,
                ctx.AllowDeletions,
                ctx.DeleteHorizonDays,
                ctx.ListFromUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                ctx.ListToUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                ctx.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            );

            await _store.BeginRunAsync(ctx.RunId, ctx.UtcNow, ct);

            _logger.LogInformation(
                "\n------------------------------\n" +
                "  Phase 1A: Getting Existing Panopto Sessions\n" +
                "------------------------------"
            );

            var returned = await _platform.Sessions.ListScheduledAsync(
                ctx.ListFromUtc,
                ctx.ListToUtc,
                ct);

            int seeded = 0;
            int aliens = 0;

            if (_store is InMemoryWorkingStore mem)
            {
                // Seed S+ sessions and register alien sessions
                mem.SeedExisting(returned, out seeded, out aliens);

                // Housekeeping: delete alien sessions *before* we do anything else.
                // For now this is always enabled; later this can be driven by config.
                await HousekeepAlienSessionsAsync(mem, ctx, enableAlienPurge: _syncOptions.Value.EnableAlienPurge, ct);
            }
            else
            {
                // Fallback counts in case a different store implementation is plugged in.
                seeded = returned.Count(r => !string.IsNullOrWhiteSpace(r.ExternalId));
                aliens = returned.Count(r => string.IsNullOrWhiteSpace(r.ExternalId));
            }

            _logger.LogInformation(
                "  Returned : {ReturnedCount}\n" +
                "  Seeded   : {SeededCount} (ExternalId present)\n" +
                "  Aliens   : {AlienCount} (no ExternalId; candidates for pre-sync purge)\n" +
                "------------------------------\n",
                returned.Count,
                seeded,
                aliens
            );
        }

        // ========================================================================
        //  P H A S E   2 —   P E R - E V E N T   S Y N C
        // ========================================================================
        public async Task SyncAsync(SourceEvent source, ScheduledSession scheduled, CancellationToken ct)
        {
            _read++;

            try
            {
                var externalId = ComputeExternalId(source);
                scheduled.Hash = externalId;

                if (string.IsNullOrWhiteSpace(externalId))
                    throw new InvalidOperationException(
                        "Unable to compute ExternalId from source event.");

                await _store.MarkSeenAsync(_ctx.RunId, externalId, externalId, ct);

                var already = await _store.GetHashAsync(externalId, ct);

                if (already != null)
                {
                    _unchanged++;

                    _logger.LogDebug("Existing match for ExternalId {ExtId}", externalId);

                    if (!_ctx.DryRun)
                    {
                        var ids = await _store.GetSessionIdsByExternalIdsAsync(new[] { externalId }, ct);
                        if (ids.Length == 1)
                        {
                            var id = ids[0];

                            await _platform.Sessions.SetExternalIdAsync(id, externalId, ct);

                            if (!string.IsNullOrWhiteSpace(scheduled.Owner))
                                await _platform.Sessions.SetOwnerAsync(id, scheduled.Owner, ct);

                            await _platform.Sessions.SetAvailabilityStartAsync(id, scheduled.StartTimeUtc, ct);
                        }
                    }

                    return;
                }

                if (_ctx.DryRun)
                {
                    _logger.LogInformation("DRYRUN → Create '{Title}' [{ExtId}]",
                        scheduled.Title, externalId);

                    await _store.UpsertHashAsync(externalId, externalId, _ctx.UtcNow, ct);
                    _upserts++;
                    return;
                }

                _logger.LogDebug("Resolving folder for '{Title}' → query '{FolderQuery}'",
                    scheduled.Title, scheduled.FolderQuery);

                var folder = await _platform.Folders.GetFolderByQuery(scheduled.FolderQuery, ct);

                if (folder is null)
                {
                    _errors++;

                    _logger.LogError(
                        "\n------------------------------\n" +
                        "  Folder resolution failed for event\n" +
                        "  Title       : {Title}\n" +
                        "  FolderQuery : {FolderQuery}\n" +
                        "  ExternalId  : {ExtId}\n" +
                        "  Action      : SKIPPED — no folder found\n" +
                        "------------------------------",
                        scheduled.Title,
                        scheduled.FolderQuery,
                        scheduled.Hash
                    );

                    return; // ← Skip this one, continue processing others
                }


                // Rob says we don't need necessarily to do this.
                // I kind of like it. It's quite belt and bracesy.
                // But what if it gives false negatives?
                // We'll find out any second once we start splatting the recorder in anyway. So let's just do that.

                //var recorder = await _platform.Recorders.GetByNameAsync(scheduled.RecorderName, ct);
                //if (recorder is null)
                //    throw new InvalidOperationException(
                //        $"Recorder not found: '{scheduled.RecorderName}'");

                //var result = await _platform.Sessions.ScheduleAsync(
                //    title: scheduled.Title,
                //    folderId: folder.Id,
                //    webcast: scheduled.Webcast == 1,
                //    startUtc: scheduled.StartTimeUtc,
                //    endUtc: scheduled.EndTimeUtc,
                //    recorderIds: new[] { recorder.Id },
                //    overwrite: true,
                //    ct);

                var result = await _platform.Sessions.ScheduleAsync(
                    title: scheduled.Title,
                    folderId: folder.Id,
                    webcast: scheduled.Webcast == 1,
                    startUtc: scheduled.StartTimeUtc,
                    endUtc: scheduled.EndTimeUtc,
                    recorderIds: new[] { Guid.Parse("27064ea5-034b-4043-9395-b31300b2cce6"),  },
                    overwrite: true,
                    ct);


                if (!result.Success || result.SessionId is null)
                    throw new InvalidOperationException(
                        $"Schedule failed: {result.LogLine}");

                var sessionId = result.SessionId.Value;

                await _platform.Sessions.SetExternalIdAsync(sessionId, externalId, ct);
                if (!string.IsNullOrWhiteSpace(scheduled.Owner))
                    await _platform.Sessions.SetOwnerAsync(sessionId, scheduled.Owner, ct);
                await _platform.Sessions.SetAvailabilityStartAsync(sessionId, scheduled.StartTimeUtc, ct);

                await _store.UpsertHashAsync(externalId, externalId, _ctx.UtcNow, ct);
                _upserts++;
            }
            catch (Exception ex)
            {
                _errors++;

                _logger.LogError(
                    ex,
                    "\n------------------------------\n" +
                    "  Sync Error: '{Title}'\n" +
                    "  ExternalId : {ExtId}\n" +
                    "------------------------------",
                    scheduled.Title,
                    scheduled.Hash
                );
            }
        }

        // ========================================================================
        //  P H A S E   3 —   C O M P L E T E   R U N
        // ========================================================================
        public async Task CompleteRunAsync(CancellationToken ct = default)
        {
            var allowDeletes = _ctx.AllowDeletions;

            if (_ctx.MinExpectedRows.HasValue && _read < _ctx.MinExpectedRows.Value)
            {
                allowDeletes = false;

                _logger.LogWarning(
                    "\n*** Low-water guard triggered ***\n" +
                    "Read={Read}  Expected≥{Expected}\n" +
                    "→ Deletions suppressed.",
                    _read,
                    _ctx.MinExpectedRows.Value
                );
            }

            var orphanIds = allowDeletes
                ? await _store.GetKeysNotSeenThisRunAsync(
                    _ctx.RunId,
                    _ctx.DeleteHorizonDays,
                    _ctx.UtcNow,
                    ct)
                : Array.Empty<string>();

            int deleted = 0;

            if (orphanIds.Length > 0)
            {
                if (_ctx.DryRun)
                {
                    _logger.LogInformation("DRYRUN → would delete {Count} sessions", orphanIds.Length);
                }
                else
                {
                    var ids = await _store.GetSessionIdsByExternalIdsAsync(orphanIds, ct);

                    if (ids.Length > 0)
                    {
                        await _platform.Sessions.DeleteAsync(ids, ct);
                        deleted = ids.Length;
                    }
                }
            }

            await _store.CompleteRunAsync(_ctx.RunId,
                _read, _upserts, _unchanged, _errors, deleted, ct);

            _logger.LogInformation(
                "\n=====================================================================\n" +
                "   S Y N C   R U N   C O M P L E T E D\n" +
                "=====================================================================\n" +
                "  Run ID         : {RunId}\n" +
                "---------------------------------------------------------------------\n" +
                "  Read           : {Read}\n" +
                "  Upserts        : {Up}\n" +
                "  Unchanged      : {Unchg}\n" +
                "  Errors         : {Err}\n" +
                "  Deleted        : {Del}\n" +
                "---------------------------------------------------------------------\n" +
                "  Dry Run        : {DryRun}\n" +
                "  Finished At    : {Now}\n" +
                "=====================================================================",
                _ctx.RunId,
                _read, _upserts, _unchanged, _errors,
                deleted,
                _ctx.DryRun,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            );
        }

        /// <summary>
        /// Computes a compact, deterministic ExternalId for a Syllabus+ event.
        /// 
        /// We use MD5 (128-bit) truncated to 96 bits (12 bytes → 24 hex chars).
        /// This comfortably fits Panopto's undocumented 40-character ExternalId limit,
        /// while leaving ample space for prefixes or metadata in future.
        /// 
        /// -----------------------------
        /// Collision Risk (Birthday Bound)
        /// -----------------------------
        /// For a 96-bit hash, the probability of a natural collision after hashing N items is:
        ///     P ≈ N² / 2^97
        ///
        /// Even with a pessimistic upper bound of:
        ///     N = 1,000,000 (one million scheduled events)
        ///
        /// the collision probability is:
        ///     P ≈ 6.3 × 10⁻¹⁸
        ///
        /// i.e. roughly:
        ///     1 in 158,000,000,000,000,000
        ///     (one in 158 quadrillion)
        ///
        /// This is far below any reasonable risk tolerance for a
        /// non-financial, non-security scheduling system such as Panopto.
        /// In practical terms, a collision is effectively impossible.
        /// </summary>
        private static string ComputeExternalId(SourceEvent e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(e.ModuleCRN);
            sb.AppendLine(e.StartDate.ToString("yyyy-MM-dd"));
            sb.AppendLine(e.StartTime.ToString(@"hh\:mm\:ss"));
            sb.AppendLine(e.LocationName);
            sb.AppendLine(e.ActivityName);
            sb.AppendLine(e.StaffUserName);

            using var md5 = MD5.Create();
            var full = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));

            // Truncate to 12 bytes (96 bits)
            var slice = new byte[12];
            Array.Copy(full, slice, 12);

            return Convert.ToHexString(slice);  // 24 chars
        }

        /// <summary>
        /// Housekeeping step executed before any Syllabus+ reconciliation:
        ///   • Identifies "alien" Panopto sessions (those with no ExternalId)
        ///   • Logs them
        ///   • Deletes them up-front (unless DryRun or purge disabled)
        /// </summary>
        /// <summary>
        /// Pre-sync alien session inspection.
        ///
        /// "Aliens" = scheduled Panopto sessions in the current horizon that have no ExternalId,
        /// so they are not known to this S+ feed.
        ///
        /// Behaviour:
        ///   • If enableAlienPurge is false: do nothing except log that inspection is disabled.
        ///   • If there are zero aliens: log and return.
        ///   • If ctx.DryRun is true: log how many alien sessions would have been acted on,
        ///     but do not call the Panopto API.
        ///   • If ctx.DryRun is false: currently we only log the count and a short explanation.
        ///     The old hard delete behaviour has been intentionally disabled after it was found
        ///     to hit personal and non-S+ sessions.
        ///
        /// Note: The config flag SyncOptions.EnableAlienPurge now effectively means
        /// "enable alien detection and logging". Physical deletion is not performed
        /// in this version.
        /// </summary>
        private async Task HousekeepAlienSessionsAsync(
            InMemoryWorkingStore mem,
            SyncRunContext ctx,
            bool enableAlienPurge,
            CancellationToken ct)
        {
            if (!enableAlienPurge)
            {
                _logger.LogInformation(
                    "Alien session inspection is disabled by configuration. " +
                    "No pre-sync checks or deletions will be performed.");
                await Task.CompletedTask;
                return;
            }

            var aliens = mem.GetAlienSessionIds();
            if (aliens.Length == 0)
            {
                _logger.LogInformation(
                    "No alien Panopto sessions detected in the current window. " +
                    "Pre-sync alien inspection completed with zero candidates.");
                await Task.CompletedTask;
                return;
            }

            if (ctx.DryRun)
            {
                _logger.LogWarning(
                    "DRYRUN → detected {Count} alien Panopto session(s) " +
                    "with no ExternalId in the current horizon. " +
                    "They would be candidates for clean-up in a destructive mode.",
                    aliens.Length);
                await Task.CompletedTask;
                return;
            }

            _logger.LogWarning(
                "\n------------------------------\n" +
                "  Pre-sync alien inspection\n" +
                "  Detected {Count} alien Panopto session(s) with no ExternalId.\n" +
                "  Rule (non-destructive mode): any scheduled session in horizon\n" +
                "        with no ExternalId is treated as non-S+ and is logged only.\n" +
                "  No deletions are performed in this build.\n" +
                "------------------------------",
                aliens.Length);

            // Important: deletion intentionally disabled for now.
            // To re-enable in future, guard this call behind a separate, explicit
            // config flag such as SyncOptions.EnableAlienDeletion and update docs.
            //
            // await _platform.Sessions.DeleteAsync(aliens, ct);

            await Task.CompletedTask;
        }


        private static bool TryParseGuid(string value, out Guid id)
            => Guid.TryParse(value, out id);
    }
}
