using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Infrastructure;

public interface ICurriculumSource
{
    Task<IEnumerable<SpRawRow>> GetRowsAsync(CancellationToken ct = default);
}