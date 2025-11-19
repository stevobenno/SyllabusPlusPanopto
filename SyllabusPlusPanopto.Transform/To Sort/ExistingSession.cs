using System;

namespace SyllabusPlusPanopto.Transform.To_Sort;

public sealed record ExistingSession(
    Guid SessionId,
    string ExternalId,   // may be empty
    DateTime StartUtc,
    DateTime EndUtc
);
