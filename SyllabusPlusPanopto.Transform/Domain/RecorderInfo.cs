using System;

namespace SyllabusPlusPanopto.Transform.Domain;

/// <summary>
/// Minimal recorder identity used by scheduling.
/// </summary>
public sealed record RecorderInfo(Guid Id, string Name);
