using System;
using System.Collections.Generic;

namespace SyllabusPlusPanopto.Transform.To_Sort;

public sealed record ScheduleResult(
    bool Success,
    Guid? SessionId,
    IReadOnlyList<Guid> ConflictingSessionIds,
    string LogLine
);
