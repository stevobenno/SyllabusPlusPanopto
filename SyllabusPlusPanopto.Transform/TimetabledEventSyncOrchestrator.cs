using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces;
using SyllabusPlusPanopto.Integration.To_Sort;

namespace SyllabusPlusPanopto.Integration
{
    /// <summary>
    /// Orchestrates Read → Transform → Sync. Host-agnostic.
    /// All defensive rules described in ISPEC §6.2/6.3/6.4 should be
    /// hooked in here (row-count plausibility, heartbeat, etc.).
    /// </summary>
    public  class TimetabledEventSyncOrchestrator : ITimetabledEventSyncOrchestrator
    {
        private readonly ISourceDataProvider _reader;
        private readonly ITransformService _transform;
        private readonly ISyncService _sync;
        private readonly ILogger<TimetabledEventSyncOrchestrator> _log;
        private readonly SyncOptions _syncOptions;

        public TimetabledEventSyncOrchestrator(
            ISourceDataProvider reader,
            ITransformService transform,
            ISyncService sync,
            IOptions<SyncOptions> syncOptions,
            ILogger<TimetabledEventSyncOrchestrator> log)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
            _sync = sync ?? throw new ArgumentNullException(nameof(sync));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _syncOptions = syncOptions?.Value ?? throw new ArgumentNullException(nameof(syncOptions));
        }
        public async Task RunAsync(CancellationToken ct = default)
        {
            var count = 0;
            var success = 0;
            var failed = 0;

            var now = DateTime.UtcNow;

            var listFromUtc = now;
            var listToUtc = now.AddDays(_syncOptions.SyncHorizonDays);

            var runCtx = new SyncRunContext(
                DryRun: false,
                MinExpectedRows: _syncOptions.MinExpectedRows,
                AllowDeletions: _syncOptions.AllowDeletions,
                DeleteHorizonDays: _syncOptions.SyncHorizonDays, // TODO: clean this if needed
                RunId: Guid.NewGuid().ToString("n"),
                UtcNow: now,
                ListFromUtc: listFromUtc,
                ListToUtc: listToUtc
            );

            // ===== RUN HEADER =====
            _log.LogInformation(
                "\n" +
                "=============================================================\n" +
                "  S Y N C   R U N   S T A R T E D\n" +
                "=============================================================\n" +
                "  Run Id          : {RunId}\n" +
                "  Dry Run         : {DryRun}\n" +
                "  Allow Deletions : {AllowDeletions}\n" +
                "  Horizon (days)  : {Horizon}\n" +
                "  Min Rows (opt)  : {MinExpected}\n" +
                "-------------------------------------------------------------\n" +
                "  Window (UTC)    : {From} → {To}\n" +
                "  Started At      : {Started}\n" +
                "=============================================================",
                runCtx.RunId,
                runCtx.DryRun,
                runCtx.AllowDeletions,
                _syncOptions.SyncHorizonDays,
                _syncOptions.MinExpectedRows?.ToString() ?? "n/a",
                runCtx.ListFromUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                runCtx.ListToUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                runCtx.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            );

            await _sync.BeginRunAsync(runCtx, ct);

            var events = _reader.ReadAsync(ct);

            await foreach (var sourceEvent in events)
            {
                ct.ThrowIfCancellationRequested();
                count++;

                try
                {

                    var scheduledSession = _transform.Transform(sourceEvent);
                    
                    await _sync.SyncAsync(sourceEvent,scheduledSession, ct);
                    success++;
                }
                catch (Exception ex)
                {
                    failed++;

                    _log.LogError(ex,
                        "Run {RunId}: failed to process row #{Row}. RawId={RawId}",
                        runCtx.RunId,
                        count,
                        sourceEvent?.StaffName);
                    // continue with the next item
                }

                if (count % 100 == 0)
                {
                    _log.LogInformation(
                        "\n" +
                        "-------------------------------------------------------------\n" +
                        "  Progress Update for Run {RunId}\n" +
                        "-------------------------------------------------------------\n" +
                        "  Processed   : {Count} rows\n" +
                        "  Successful  : {Success}\n" +
                        "  Failed      : {Failed}\n" +
                        "-------------------------------------------------------------",
                        runCtx.RunId,
                        count,
                        success,
                        failed
                    );
                }
            }

            await _sync.CompleteRunAsync(ct);

            var completedAt = DateTime.UtcNow;
            var duration = completedAt - runCtx.UtcNow;

            // ===== RUN SUMMARY =====
            _log.LogInformation(
                "\n" +
                "=============================================================\n" +
                "  S Y N C   R U N   C O M P L E T E\n" +
                "=============================================================\n" +
                "  Run Id          : {RunId}\n" +
                "-------------------------------------------------------------\n" +
                "  Total Rows      : {Total}\n" +
                "  Successful      : {Success}\n" +
                "  Failed          : {Failed}\n" +
                "-------------------------------------------------------------\n" +
                "  Started At (UTC): {Started}\n" +
                "  Completed At    : {Completed}\n" +
                "  Duration        : {DurationMs} ms\n" +
                "=============================================================",
                runCtx.RunId,
                count,
                success,
                failed,
                runCtx.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                completedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                (int)duration.TotalMilliseconds
            );
        }

    }
}
