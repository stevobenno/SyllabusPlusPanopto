using System.Collections.Generic;
using System.Threading;
using SyllabusPlusPanopto.Domain;
using SyllabusPlusPanopto.Transform.Domain;

namespace SyllabusPlusPanopto.Transform.Interfaces;

public interface ISourceDataProvider
{
    IAsyncEnumerable<SourceEvent> ReadAsync(CancellationToken ct = default);
}
