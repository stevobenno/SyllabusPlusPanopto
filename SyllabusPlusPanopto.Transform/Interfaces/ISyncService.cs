using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;
using SyncRunContext = SyllabusPlusPanopto.Integration.To_Sort.SyncRunContext;

namespace SyllabusPlusPanopto.Integration.Interfaces;

public interface ISyncService
{
    Task BeginRunAsync(SyncRunContext ctx, CancellationToken ct = default);
    Task SyncAsync(ScheduledSession session, CancellationToken ct = default);
    Task CompleteRunAsync(CancellationToken ct = default);
}
