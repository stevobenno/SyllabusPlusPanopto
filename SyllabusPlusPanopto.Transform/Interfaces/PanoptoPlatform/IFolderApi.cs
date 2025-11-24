using System;
using System.Threading;
using System.Threading.Tasks;
using SyllabusPlusPanopto.Integration.Domain;

namespace SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

/// <summary>
/// Folder lookup by name or id.
/// </summary>
public interface IFolderApi
{
    /// <summary>Lookup by exact folder name (case-insensitive where supported). Returns null if not found.</summary>
    Task<FolderInfo> GetFolderByQuery(string folderQuery, CancellationToken ct);

    /// <summary>Lookup by folder id. Returns null if not found.</summary>
    Task<FolderInfo?> GetByIdAsync(Guid id, CancellationToken ct);
}

