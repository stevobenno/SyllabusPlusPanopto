using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Application;

/// <summary>
/// Thin orchestration contract used by both Console and Functions hosts.
/// </summary>
public interface IProcessFlow
{
    Task RunAsync(bool dryRun = false, CancellationToken ct = default);
}