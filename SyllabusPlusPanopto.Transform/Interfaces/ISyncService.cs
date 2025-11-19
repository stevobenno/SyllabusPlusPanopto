using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Transform.Domain;
using SyncRunContext = SyllabusPlusPanopto.Transform.To_Sort.SyncRunContext;

namespace SyllabusPlusPanopto.Transform.Interfaces;

public interface ISyncService
{
    Task BeginRunAsync(SyncRunContext ctx, CancellationToken ct = default);
    Task SyncAsync(ScheduledSession session, CancellationToken ct = default);
    Task CompleteRunAsync(CancellationToken ct = default);
}
