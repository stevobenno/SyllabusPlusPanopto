using System.Collections.Generic;
using System.Threading;
using SyllabusPlusPanopto.Integration.Domain;

namespace SyllabusPlusPanopto.Integration.Interfaces;

public interface ISourceDataProvider
{
    IAsyncEnumerable<SourceEvent> ReadAsync(CancellationToken ct = default);
}
