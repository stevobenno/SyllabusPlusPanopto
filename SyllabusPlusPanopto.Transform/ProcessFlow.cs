// SyllabusPlusPanopto.Application/ProcessFlow.cs

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SyllabusPlusPanopto.Application;
using SyllabusPlusPanopto.Transform.Interfaces;

// ISourceDataProvider (reader)
// ITransformService (mapping)  -> adjust to your namespace

// ISyncService (Panopto ops)   -> adjust to your namespace

namespace SyllabusPlusPanopto.Transform
{
    /// <summary>
    /// Orchestrates Read → Transform → Sync. Contains no host-specific logic.
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

            await foreach (var raw in _reader.ReadAsync(ct))
            {
                ct.ThrowIfCancellationRequested();

                var mapped = _transform.Transform(raw);

                if (dryRun)
                {
                    _log.LogInformation("DRYRUN {Key} {Owner}", mapped.Key, mapped.Raw.OwnerEmail);
                }
                else
                {
                    await _sync.SyncAsync(mapped, ct);
                }

                if (++count % 100 == 0)
                    _log.LogInformation("Processed {Count}", count);
            }

            _log.LogInformation("Completed {Count} item(s).", count);
        }
    }
}
