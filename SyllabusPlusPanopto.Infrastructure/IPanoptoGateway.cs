using System;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Infrastructure;

public interface IPanoptoGateway
{
    Task<Guid?> GetSessionByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<Guid> CreateSessionAsync(PanoptoScheduleCandidate candidate, CancellationToken ct = default);
    Task UpdateSessionAsync(Guid sessionId, PanoptoScheduleCandidate candidate, CancellationToken ct = default);
}