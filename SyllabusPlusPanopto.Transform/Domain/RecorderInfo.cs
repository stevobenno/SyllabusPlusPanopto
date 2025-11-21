using System;

namespace SyllabusPlusPanopto.Integration.Domain;

/// <summary>
/// Minimal recorder identity used by scheduling.
/// </summary>
public sealed record RecorderInfo(Guid Id, string Name);
