
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Infrastructure;

using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Domain;

public interface ICurriculumSource
{
    Task<IEnumerable<SpRawRow>> GetRowsAsync(CancellationToken ct = default);
}

public interface IPanoptoGateway
{
    Task<Guid?> GetSessionByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<Guid> CreateSessionAsync(PanoptoScheduleCandidate candidate, CancellationToken ct = default);
    Task UpdateSessionAsync(Guid sessionId, PanoptoScheduleCandidate candidate, CancellationToken ct = default);
}

public interface IEventPublisher
{
    Task PublishAsync(string topic, object payload, string? messageId = null, CancellationToken ct = default);
}

// Minimal options examples
public class SqlOptions
{
    public string? ConnectionString { get; set; }
}

public class ServiceBusOptions
{
    public string? ConnectionString { get; set; }
}
