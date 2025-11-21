using System;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public sealed record SyncRunContext(
    bool DryRun,
    int? MinExpectedRows,
    bool AllowDeletions,
    int DeleteHorizonDays,
    string RunId,
    DateTime UtcNow,
    DateTime ListFromUtc,   // scope for pre-run enumeration
    DateTime ListToUtc      // scope for pre-run enumeration
);
