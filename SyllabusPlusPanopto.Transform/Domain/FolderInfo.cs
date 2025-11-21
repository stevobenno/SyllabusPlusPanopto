using System;

namespace SyllabusPlusPanopto.Integration.Domain;

/// <summary>
/// Minimal folder identity used by scheduling.
/// </summary>
public sealed record FolderInfo(Guid Id, string Name);
