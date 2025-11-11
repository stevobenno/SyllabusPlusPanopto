// SyllabusPlusPanopto.Application/ProcessFlow.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform
{
    /// <summary>
    /// Orchestrates Read → Transform → Sync. Host-agnostic.
    /// All defensive rules described in ISPEC §6.2/6.3/6.4 should be
    /// hooked in here (row-count plausibility, heartbeat, etc.).
    /// </summary>
    internal sealed class ProcessFlow : IProcessFlow
    {
        private readonly ISourceDataProvider _reader;
        private readonly ITransformService _transform;
        private readonly ISyncService _sync;
        private readonly ILogger<ProcessFlow> _log;

        public ProcessFlow(
            ISourceDataProvider reader,
            ITransformService transform,
            ISyncService sync,
            ILogger<ProcessFlow> log)
        {
            _reader = reader;
            _transform = transform;
            _sync = sync;
            _log = log;
        }

        public async Task RunAsync(bool dryRun = false, CancellationToken ct = default)
        {
            var count = 0;
            var success = 0;
            var failed = 0;

            // TODO: if the provider can tell us "I will return N rows", fetch that first
            // and apply the "plausibility / minimum threshold" rule from the design
            // (ISPEC §6.3). For now we just stream.

            await foreach (var raw in _reader.ReadAsync(ct))
            {
                ct.ThrowIfCancellationRequested();
                count++;

                try
                {
                    // 1. transform S+ row to canonical DTO (Automapper profile)
                    var mapped = _transform.Transform(raw);

                    if (dryRun)
                    {
                        // minimal output now; later we can include the hash + resolved folder
                        _log.LogInformation("DRYRUN {Key} {Owner}", mapped.Title, mapped.Raw.StaffName);
                    }
                    else
                    {
                        // 2. push to Panopto sync logic (this will do the
                        //    hash compare + create/delete described in §6.4/6.5)
                        await _sync.SyncAsync(mapped, ct);
                    }

                    success++;
                }
                catch (Exception ex)
                {
                    failed++;

                    // TODO: promote this to structured logging with the hash/correlation ID
                    // so we can trace it in App Insights as per §6.8
                    _log.LogError(ex,
                        "Failed to process row #{Row}. RawId={RawId}",
                        count,
                        raw?.Id /* or whatever field identifies the S+ row */);
                    // continue with the next item
                }

                if (count % 100 == 0)
                {
                    _log.LogInformation("Processed {Count} item(s). Success={Success} Failed={Failed}",
                        count, success, failed);
                }
            }

            // TODO: at this point apply the "if count < MinExpectedRowos ⇒ raise alert and
            // DO NOT apply deletions" rule. Right now we just log.
            _log.LogInformation("Completed run. Total={Total} Success={Success} Failed={Failed}",
                count, success, failed);
        }
    }
}
