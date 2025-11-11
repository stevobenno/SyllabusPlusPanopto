using System;

namespace SyllabusPlusPanopto.Transform.Domain
{
    /// <summary>
    /// Represents one raw record extracted from the Argos/Syllabus Plus reporting view.
    /// Property names match the Argos CSV header exactly to simplify CSV→object mapping.
    /// </summary>
    public record SourceEvent(
        string ActivityName,     // SPlus Data!A – used in Description text
        string ModuleCode,       // SPlus Data!B – appears in Title
        string ModuleName,       // SPlus Data!C – optional, not currently mapped
        string ModuleCRN,        // SPlus Data!D – used in Folder logic
        string StaffName,        // SPlus Data!E – used in Description
        DateTime StartDate,      // SPlus Data!F
        TimeSpan StartTime,      // SPlus Data!G
        TimeSpan EndTime,        // SPlus Data!H
        string LocationName,     // SPlus Data!I
        string RecorderName,     // SPlus Data!J
        int RecordingFactor,     // SPlus Data!K (1–5 → webcast flag)
        string StaffUserName     // SPlus Data!L
    );
}
