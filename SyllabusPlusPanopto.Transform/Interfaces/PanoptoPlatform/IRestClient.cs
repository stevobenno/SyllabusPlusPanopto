using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

/// <summary>
/// Simple abstraction for HTTP transport (can be HttpClient, Refit, or your in-house wrapper).
/// </summary>
public interface IRestClient
{
    Task<T> GetAsync<T>(string path, CancellationToken ct);
    Task<T> PostAsync<T>(string path, object payload, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}
