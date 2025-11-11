using AutoMapper;
using System;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.TransformationServices.Mappers.MapHelpersResolversBuilders;

// wherever SpRawRow lives

namespace SyllabusPlusPanopto.Transform.TransformationServices.Mappers
{
    /// <summary>
    /// Maps the Argos / S+ CSV row into the canonical scheduling DTO
    /// using exactly the rules captured in:
    /// - "MEL Schedule Maker - Working.xlsx"
    /// - the chat with Robert Moores (Title, Recorder, Folder, Webcast)
    /// Every mapping below references the relevant Excel formula so future
    /// devs can confirm we did not invent anything.
    /// </summary>
    public sealed class SPlusToPanoptoProfile : Profile
    {
        public SPlusToPanoptoProfile()
        {
            CreateMap<SourceEvent, ScheduledSession>()
                // ------------------------------------------------------------------
                // TITLE
                // Excel (from Rob):
                // =CONCAT('SPlus Data'!B2," ",TEXT('SPlus Data'!F2,"dd/mm/yyyy")," ",
                //         TEXT('SPlus Data'!G2,"hh:mm")," ",'SPlus Data'!I2)
                // i.e.
                //   Title = <ModuleCode> <StartDate dd/MM/yyyy> <StartTime HH:mm> <LocationName>
                // Example:
                //   CIVE5331M01 30/10/2025 09:00 Civil Engineering TR (3.08)
                // ------------------------------------------------------------------
                .ForMember(d => d.Title,
                    m => m.MapFrom(s =>
                        $"{FirstToken(s.ModuleCode)} {s.StartDate:dd/MM/yyyy} {s.StartTime:hh\\:mm} {s.LocationName}".Trim()))

                // ------------------------------------------------------------------
                // RECORDER NAME
                // Excel (from Rob):
                // =IF(ISBLANK('SPlus Data'!J2),
                //      XLOOKUP('SPlus Data'!I2,Variables!$A$24:$A$25,Variables!$B$24:$B$25,Variables!$B$10),
                //      ('SPlus Data'!J2))
                //
                // Meaning:
                // 1. If S+ gives us a recorder name (column J) -> use it.
                // 2. Else, use the location (column I) to look up the recorder from a
                //    small "cluster" list (Variables sheet).
                // 3. Else, use a default.
                // Because we are *not* keeping a variables sheet in code, we call a
                // helper that implements the same precedence.
                // ------------------------------------------------------------------
                .ForMember(d => d.RecorderName,
                    m => m.MapFrom(s => RecorderResolver.Resolve(s.RecorderName, s.LocationName)))

                // ------------------------------------------------------------------
                // START TIME (UTC, with start offset)
                // Excel:
                //   Start = SPlus.StartTime + TimeDiffUTC + StartTimeOffset
                // In the workbook the time diff is 00:00 and the start offset is 00:02,
                // i.e. "start 2 mins after timetable start".
                // We capture the *idea* here: apply a known offset and convert to UTC.
                // ------------------------------------------------------------------
                .ForMember(d => d.StartTimeUtc,
                    m => m.MapFrom(s =>
                        TimeHelper.ToUtcWithStartOffset(s.StartDate, s.StartTime)))

                // ------------------------------------------------------------------
                // END TIME (UTC, with end offset)
                // Excel:
                //   End = SPlus.EndTime + TimeDiffUTC - EndTimeOffset
                // Workbook uses 2 mins, i.e. "finish 2 mins before timetable end".
                // ------------------------------------------------------------------
                .ForMember(d => d.EndTimeUtc,
                    m => m.MapFrom(s =>
                        TimeHelper.ToUtcWithEndOffset(s.StartDate, s.EndTime)))

                // ------------------------------------------------------------------
                // DESCRIPTION / PRESENTER TEXT
                // Excel logic:
                //   text = Variables!B24 ("The full name of this activity is: ")
                //          + ActivityName
                //          + IF(StaffName blank,
                //               Variables!B23 ("No Presenter name has been provided"),
                //               Variables!B22 ("The presenter(s) named for this event are: ") + StaffName)
                //
                // We hard-code the same English text here so if someone opens the Excel
                // and this code side-by-side, they see it’s the same rule.
                // ------------------------------------------------------------------
                .ForMember(d => d.Description,
                    m => m.MapFrom(s => DescriptionBuilder.Build(s.ActivityName, s.StaffName)))

                // ------------------------------------------------------------------
                // FOLDER NAME
                // Excel LET() monster (summarised):
                //   dVal = ModuleCRN (possibly comma-separated)
                //   key  = first token of dVal
                //   if dVal starts with "#SPLUS" -> use staff personal folder
                //   else try to find module folder in "Panopto Data"
                //   else use default "25/26 Recording Catchall"
                //
                // We are *not* doing folder creation here. We replicate:
                //   - first CRN wins
                //   - #SPLUS events go to staff personal if present, else default
                //   - otherwise convention-based module folder name, else default
                // ------------------------------------------------------------------
                .ForMember(d => d.FolderName,
                    m => m.MapFrom(s =>
                        FolderNameBuilder.FromSyllabusPlus(
                            moduleCrn: s.ModuleCRN,
                            staffUser: s.StaffUserName)))

                // ------------------------------------------------------------------
                // WEBCAST FLAG
                // Excel:
                //   S+ "Factor" column:
                //   1 – record all
                //   2 – record slides + audio
                //   3 – do not record
                //   4 – as 1 + broadcast
                //   5 – as 2 + broadcast
                // CSV wants 1 or 0.
                // So: webcast = 1 when factor is 4 or 5, else 0.
                // ------------------------------------------------------------------
                .ForMember(d => d.Webcast,
                    m => m.MapFrom(s =>
                        RecordingFactorMapper.ToWebcastFlag(s.RecordingFactor)))

                // ------------------------------------------------------------------
                // OWNER
                // Excel:
                //   if StaffUserName is blank:
                //       owner = scheduler@leeds.ac.uk (Variables!B20 + Variables!B19)
                //   else:
                //       take first StaffUserName (before comma), prepend "unified\"
                // ------------------------------------------------------------------
                .ForMember(d => d.Owner,
                    m => m.MapFrom(s =>
                        OwnerResolver.ResolveOwner(s.StaffUserName)))
                ;
        }

        private static string FirstToken(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var idx = input.IndexOf(',', StringComparison.Ordinal);
            return idx > 0 ? input[..idx].Trim() : input.Trim();
        }
    }

   
}

