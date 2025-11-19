using System;

namespace SyllabusPlusPanopto.Transform.Domain;

/// <summary>
/// Minimal folder identity used by scheduling.
/// </summary>
public sealed record FolderInfo(Guid Id, string Name);
