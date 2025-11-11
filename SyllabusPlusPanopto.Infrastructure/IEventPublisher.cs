using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Infrastructure;

public interface IEventPublisher
{
    Task PublishAsync(string topic, object payload, string? messageId = null, CancellationToken ct = default);
}