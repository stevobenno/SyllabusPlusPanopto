using System;

namespace SyllabusPlusPanopto.Integration.Domain
{
    /// <summary>
    /// Canonical, transformed, Panopto-ready DTO.
    /// Every property is derived deterministically from the Argos/S+ row,
    /// following the rules in "MEL Schedule Maker - Working.xlsx".
    /// This is the thing we will hash and compare against Panopto's ExternalId.
    /// </summary>
    public sealed class ScheduledSession
    {
        /// <summary>
        /// Title built as per Excel:
        /// =CONCAT(ModuleCode, " ", StartDate(dd/MM/yyyy), " ", StartTime(hh:mm), " ", LocationName)
        /// Example: "CIVE5331M01 30/10/2025 09:00 Civil Engineering TR (3.08)"
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Start time in UTC with the 2-min start offset applied (see TimeHelper).
        /// </summary>
        public DateTime StartTimeUtc { get; init; }

        /// <summary>
        /// End time in UTC with the 2-min early finish applied (see TimeHelper).
        /// </summary>
        public DateTime EndTimeUtc { get; init; }

        /// <summary>
        /// Recorder name as resolved from S+ (explicit) or from the room/cluster
        /// rule in the spreadsheet.
        /// </summary>
        public string RecorderName { get; init; } = string.Empty;

        /// <summary>
        /// Folder name resolved by convention:
        /// - first CRN wins
        /// - #SPLUS → staff personal folder or default
        /// - else convention folder name
        /// </summary>
        public string FolderName { get; init; } = string.Empty;

        /// <summary>
        /// Human-friendly description, same text as in the workbook:
        /// "The full name of this activity is: ... The presenter(s) named ..."
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// 0 or 1; 1 only when S+ RecordingFactor is 4 or 5.
        /// </summary>
        public int Webcast { get; init; }

        /// <summary>
        /// Owner to set on the session; either unified\{staff} or scheduler@leeds.ac.uk
        /// as per workbook Variables sheet.
        /// </summary>
        public string Owner { get; init; } = string.Empty;

        /// <summary>
        /// Will be filled by the hashing step later (MD5→base36 or truncated SHA).
        /// Kept here so we can log it alongside all the other fields.
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Keep the original row for audit/troubleshooting.
        /// This mirrors the "Raw" idea in your ProcessFlow.
        /// </summary>
        public SourceEvent Raw { get; set; } = null!;
    }
}
