using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Transform.Interfaces;

/// <summary>
/// Thin orchestration contract used by both Console and Functions hosts.
/// </summary>
public interface ITimetabledEventSyncOrchestrator
{
    Task RunAsync( CancellationToken ct = default);
}
