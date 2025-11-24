using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;
using SyncRunContext = SyllabusPlusPanopto.Integration.To_Sort.SyncRunContext;

namespace SyllabusPlusPanopto.Integration.Interfaces;

public interface ISyncService
{
    Task BeginRunAsync(SyncRunContext ctx, CancellationToken ct = default);

    Task SyncAsync(SourceEvent source, ScheduledSession scheduled, CancellationToken ct);

    Task CompleteRunAsync(CancellationToken ct = default);
}
