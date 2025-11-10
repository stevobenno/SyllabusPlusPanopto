using System.Collections.Generic;
using System.Threading;
using SyllabusPlusPanopto.Domain;

namespace SyllabusPlusPanopto.Transform.Interfaces;

public interface ISourceDataProvider
{
    IAsyncEnumerable<SpRawRow> ReadAsync(CancellationToken ct = default);
}
