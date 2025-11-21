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

            var listFromUtc = now.AddDays(-_syncOptions.LookbackDays);
            var listToUtc = now;

            var runCtx = new SyncRunContext(
                DryRun: false,                                 
                MinExpectedRows: _syncOptions.MinExpectedRows,
                AllowDeletions: _syncOptions.AllowDeletions,
                DeleteHorizonDays: _syncOptions.DeleteHorizonDays,
                RunId: Guid.NewGuid().ToString("n"),
                UtcNow: now,
                ListFromUtc: listFromUtc,
                ListToUtc: listToUtc
            );

            _log.LogInformation(
                "Starting sync run {RunId}. Window: {From} → {To}. AllowDeletions={AllowDeletions}",
                runCtx.RunId, runCtx.ListFromUtc, runCtx.ListToUtc, runCtx.AllowDeletions);

            await _sync.BeginRunAsync(runCtx, ct);

            await foreach (var sourceEvent in _reader.ReadAsync(ct))
            {
                ct.ThrowIfCancellationRequested();
                count++;

                try
                {
                    var mapped = _transform.Transform(sourceEvent);
                    await _sync.SyncAsync(mapped, ct);
                    success++;
                }
                catch (Exception ex)
                {
                    failed++;

                    _log.LogError(ex,
                        "Failed to process row #{Row}. RawId={RawId}",
                        count,
                        sourceEvent?.StaffName);

                    // continue with the next item
                }

                if (count % 100 == 0)
                {
                    _log.LogInformation("Processed {Count} item(s). Success={Success} Failed={Failed}",
                        count, success, failed);
                }
            }

            // Here you can enforce plausibility checks based on _syncOptions.MinExpectedRows
            // and _syncOptions.AllowDeletions if you want to alter deletion behaviour.
            //
            // e.g.:
            //
            // if (_syncOptions.MinExpectedRows.HasValue &&
            //     count < _syncOptions.MinExpectedRows.Value)
            // {
            //     _log.LogWarning(
            //         "Row-count plausibility check failed: expected ≥ {Expected}, got {Actual}. " +
            //         "Deletions should be suppressed for this run.",
            //         _syncOptions.MinExpectedRows.Value, count);
            // }

            await _sync.CompleteRunAsync(ct);

            _log.LogInformation(
                "Completed run {RunId}. Total={Total} Success={Success} Failed={Failed}",
                runCtx.RunId, count, success, failed);
        }
    }
}
