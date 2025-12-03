using System;
using SessionManagement;

namespace SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;

public interface ISessionManagementWrapper
{
    Folder GetFolderByName(string folderName);

    Folder GetFolderByQuery(string query);
    Folder GetFolderById(Guid id);
    Session[] GetSessionsInDateRange(DateTime start, DateTime end);
    Session[] GetSessionById(Guid id);
    bool TryGetSessionId(string sessionName, out Guid sessionId);

    void UpdateSessionOwner(Guid[] sessionIds, string ownerUserKey);
    void UpdateSessionsAvailabilityStart(Guid[] sessionIds, DateTime availabilityUtc);

    /// <summary>
    /// Returns the current ExternalId for a single session (string.Empty if not found).
    /// </summary>
    string GetExternalId(Guid sessionId);

    /// <summary>
    /// Replaces the ExternalId for a single session (thin wrapper).
    /// </summary>
    void UpdateSessionExternalId(Guid id, string externalId);

    /// <summary>
    /// (Back-compat name) Returns ExternalId (we no longer use Description for flags).
    /// </summary>
    string GetDescription(Guid sessionId);

    /// <summary>
    /// (Back-compat name) Writes ExternalId (we no longer use Description for flags).
    /// </summary>
    void UpdateSessionDescription(Guid id, string descriptionLikeButActuallyExternalId);

    /// <summary>
    /// Remove our processed marker(s) from ExternalId.
    /// </summary>
    bool RemoveProcessedMarker(Guid sessionId, string flagToken);

    /// <summary>
    /// Appends a processed marker to ExternalId if it isn't already present.
    /// Returns true if an update was performed, false if it was already marked.
    /// </summary>
    bool AppendProcessedMarker(Guid sessionId, string markerPrefix, string timestampFormat = "o");

    bool DeleteSessions(Guid[] sessionIds);


    bool IsOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2);
    void Dispose();
}
