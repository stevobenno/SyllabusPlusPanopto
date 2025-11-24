using System;

namespace SyllabusPlusPanopto.Integration.Domain
{
    /// <summary>
    /// Canonical, transformed, Panopto-ready DTO.
    ///
    /// Every property is derived deterministically from the Argos/S+ row,
    /// following the rules in "MEL Schedule Maker - Working.xlsx".
    ///
    /// This is the thing we will hash and compare against Panopto's ExternalId.
    /// </summary>
    public sealed class ScheduledSession
    {
        /// <summary>
        /// Title built as per Excel:
        ///
        ///   =CONCAT(
        ///       ModuleCode, " ",
        ///       StartDate(dd/MM/yyyy), " ",
        ///       StartTime(hh:mm), " ",
        ///       LocationName
        ///   )
        ///
        /// Example:
        ///   "CIVE5331M01 30/10/2025 09:00 Civil Engineering TR (3.08)"
        ///
        /// This is what the lecturer and students will actually see in Panopto.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Start time in UTC with the 2-minute start offset applied
        /// (see TimeHelper / workbook rules).
        /// </summary>
        public DateTime StartTimeUtc { get; init; }

        /// <summary>
        /// End time in UTC with the 2-minute early finish applied
        /// (see TimeHelper / workbook rules).
        /// </summary>
        public DateTime EndTimeUtc { get; init; }

        /// <summary>
        /// Recorder name as resolved from S+ (explicit) or via the
        /// room/cluster mapping rules in the workbook.
        ///
        /// This must match a named recorder in Panopto (via SOAP).
        /// </summary>
        public string RecorderName { get; init; } = string.Empty;

        /// <summary>
        /// FolderQuery is the token we get from S+ that we use to locate
        /// the actual Panopto folder.
        ///
        /// Important:
        ///  - This is NOT guaranteed to be the literal Panopto folder name.
        ///  - In practice it will be a CRN / module identifier (e.g. "1-23459")
        ///    which appears somewhere in the Panopto folder path.
        ///
        /// Resolution rules (implemented in the Folder API / SyncService):
        ///  - Use FolderQuery as a search token against Panopto folders
        ///  - Perform a wildcard / "contains" search on folder name / path
        ///  - Pick the best match (typically first exact/contains match)
        ///
        /// The resolved folder name / Id are captured separately.
        /// </summary>
        public string FolderQuery { get; init; } = string.Empty;

        /// <summary>
        /// Resolved Panopto folder name (optional).
        ///
        /// This is set once FolderQuery has been resolved against Panopto,
        /// and is mainly for logging, diagnostics and audit trails.
        /// </summary>
        public string? ResolvedFolderName { get; set; }

        /// <summary>
        /// Resolved Panopto folder Id (optional).
        ///
        /// This is what actually gets used when calling the Panopto APIs
        /// to schedule the session.
        /// </summary>
        public Guid? ResolvedFolderId { get; set; }

        /// <summary>
        /// Human-friendly description, same text as in the workbook:
        ///
        ///   "The full name of this activity is: ... The presenter(s) named ..."
        ///
        /// This is optional from Panopto's perspective but helps support and
        /// end-users understand what the session actually is.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// 0 or 1; 1 only when S+ RecordingFactor is 4 or 5.
        ///
        /// This controls whether the Panopto session is created as a webcast
        /// as well as a recording.
        /// </summary>
        public int Webcast { get; init; }

        /// <summary>
        /// Owner to set on the session; typically one of:
        ///   - unified\{staffUserName}
        ///   - scheduler@leeds.ac.uk
        ///
        /// The exact rules are taken from the workbook "Variables" sheet.
        /// </summary>
        public string Owner { get; init; } = string.Empty;

        /// <summary>
        /// External identity hash for reconciliation with Panopto.
        ///
        /// Implementation (current):
        ///   - Based on a deterministic subset of S+ fields
        ///     (CRN, date, time, location, activity, staff user name, etc.)
        ///   - MD5(…) computed over that canonical string
        ///   - Truncated to the first 12 hexadecimal characters
        ///     → 48 bits of effective hash
        ///
        /// Usage:
        ///   - Written into Panopto's ExternalId field
        ///   - Used as the key in our in-memory working store
        ///   - Drives add/update/delete decisions during sync
        ///
        /// Notes:
        ///   - This is *not* used for cryptographic security.
        ///   - 48 bits (12 hex chars) still gives a very low collision
        ///     probability for the scale of a university timetable.
        ///   - Kept on the DTO so we can log it alongside the rest of the
        ///     session fields for troubleshooting.
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Original S+/Argos row for audit/troubleshooting.
        ///
        /// This mirrors the "Raw" concept in the ProcessFlow:
        ///   - if anything looks wrong in Panopto, we can always
        ///     inspect the underlying SourceEvent that produced it.
        /// </summary>
        public SourceEvent Raw { get; set; } = null!;
    }
}
