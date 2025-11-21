using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;

namespace SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

/// <summary>
/// Recorder discovery and lookup.
/// </summary>
public interface IRecorderApi
{
    /// <summary>
    /// Resolve a recorder by name (case-insensitive where supported).
    /// Returns null if not found.
    /// </summary>
    Task<RecorderInfo> GetByNameAsync(string recorderName, CancellationToken ct);
}
