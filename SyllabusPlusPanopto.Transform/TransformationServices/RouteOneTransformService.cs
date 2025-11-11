using System;
using System.Collections.Generic;
using SyllabusPlusPanopto.Transform.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;

namespace SyllabusPlusPanopto.Transform.TransformationServices
{
    /// <summary>
    /// Horribly explicit, no-AutoMapper implementation of the spreadsheet rules.
    /// This exists so that people who "don't like magic" can see every line.
    /// Every field below references the original Excel logic Rob shared.
    /// </summary>
    public sealed class RouteOneTransformService : ITransformService
    {
        // spreadsheet constants
        private const string DescriptionActivityPrefix = "The full name of this activity is: ";
        private const string DescriptionNoPresenter = "No Presenter name has been provided";
        private const string DescriptionPresenterPrefix = "The presenter(s) named for this event are: ";

        private const string DefaultFolder = "Recording Catchall";
        private const string UnifiedPrefix = "unified\\";
        private const string SchedulerAccount = "scheduler@leeds.ac.uk";

        // in the workbook both are 2 minutes
        private static readonly TimeSpan StartOffset = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan EndOffset = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan UtcDiff = TimeSpan.Zero; // workbook has 00:00

        public ScheduledSession Transform(SourceEvent sourceEvent)
        {
            if (sourceEvent == null) throw new ArgumentNullException(nameof(sourceEvent));

            // --------------------------------------------------------------------
            // TITLE
            // =CONCAT(ModuleCode, " ", StartDate(dd/MM/yyyy), " ", StartTime(hh:mm), " ", LocationName)
            // --------------------------------------------------------------------
            var firstModuleCode = FirstToken(sourceEvent.ModuleCode);
            var title = $"{firstModuleCode} {sourceEvent.StartDate:dd/MM/yyyy} {sourceEvent.StartTime:hh\\:mm} {sourceEvent.LocationName}".Trim();

            // --------------------------------------------------------------------
            // START/END UTC
            // Start = SPlus.StartTime + 00:02 + utcDiff
            // End   = SPlus.EndTime   - 00:02 + utcDiff
            // --------------------------------------------------------------------
            var startUtc = DateTime.SpecifyKind(
                sourceEvent.StartDate.Date + sourceEvent.StartTime + StartOffset + UtcDiff,
                DateTimeKind.Utc);

            var endUtc = DateTime.SpecifyKind(
                sourceEvent.StartDate.Date + sourceEvent.EndTime - EndOffset + UtcDiff,
                DateTimeKind.Utc);

            // --------------------------------------------------------------------
            // RECORDER
            // =IF(ISBLANK(Recorder), XLOOKUP(Location,...), Recorder)
            // We don't have the lookup table here, so: recorder || location || "UNKNOWN_RECORDER"
            // --------------------------------------------------------------------
            var recorder = !string.IsNullOrWhiteSpace(sourceEvent.RecorderName)
                ? sourceEvent.RecorderName.Trim()
                : (!string.IsNullOrWhiteSpace(sourceEvent.LocationName)
                    ? sourceEvent.LocationName.Trim()
                    : "UNKNOWN_RECORDER");

            // --------------------------------------------------------------------
            // DESCRIPTION
            // Base: "The full name of this activity is: " + ActivityName
            // Then either:
            //   ". No Presenter name has been provided"
            // or
            //   ". The presenter(s) named for this event are: " + StaffName
            // --------------------------------------------------------------------
            var descriptionBase = DescriptionActivityPrefix + (sourceEvent.ActivityName ?? string.Empty);
            string description;
            if (string.IsNullOrWhiteSpace(sourceEvent.StaffName))
            {
                description = descriptionBase + ". " + DescriptionNoPresenter;
            }
            else
            {
                description = descriptionBase + ". " + DescriptionPresenterPrefix + sourceEvent.StaffName;
            }

            // --------------------------------------------------------------------
            // FOLDER
            // LET() in Excel:
            //   - take first ModuleCRN
            //   - if starts with "#SPLUS" → staff personal (unified\user) else default
            //   - else try module folder else default
            // We don't have Panopto Data lookup here, so we do convention only.
            // --------------------------------------------------------------------
            var folder = ResolveFolder(sourceEvent.ModuleCRN, sourceEvent.StaffUserName);

            // --------------------------------------------------------------------
            // WEBCAST
            // factor 4 or 5 → 1 else 0
            // --------------------------------------------------------------------
            var webcast = (sourceEvent.RecordingFactor == 4 || sourceEvent.RecordingFactor == 5) ? 1 : 0;

            // --------------------------------------------------------------------
            // OWNER
            // if StaffUserName blank → scheduler@leeds.ac.uk
            // else → unified\{first staff username}
            // --------------------------------------------------------------------
            var owner = ResolveOwner(sourceEvent.StaffUserName);

            // Hash will be filled by the hashing step, not here.
            return new ScheduledSession
            {
                Title = title,
                StartTimeUtc = startUtc,
                EndTimeUtc = endUtc,
                RecorderName = recorder,
                FolderName = folder,
                Description = description,
                Webcast = webcast,
                Owner = owner,
                Raw = sourceEvent
            };
        }

        public IReadOnlyList<ScheduledSession> Transform(IEnumerable<SourceEvent> sourceEvents)
        {
            if (sourceEvents == null) throw new ArgumentNullException(nameof(sourceEvents));
            var list = new List<ScheduledSession>();
            foreach (var e in sourceEvents)
            {
                list.Add(Transform(e));
            }
            return list;
        }

        // --------------------------------------------------------------------
        // helpers (these are the same as we used in the AutoMapper version,
        // just inlined here to make the "I don't like AutoMapper" crowd happy)
        // --------------------------------------------------------------------

        private static string FirstToken(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var idx = input.IndexOf(',', StringComparison.Ordinal);
            return idx > 0 ? input[..idx].Trim() : input.Trim();
        }

        private static string ResolveFolder(string? moduleCrn, string? staffUser)
        {
            var firstCrn = FirstToken(moduleCrn);

            // Excel: IF(LEFT(dVal, 6) = "#SPLUS", ...)
            if (!string.IsNullOrWhiteSpace(firstCrn) &&
                firstCrn.StartsWith("#SPLUS", StringComparison.OrdinalIgnoreCase))
            {
                var firstStaff = FirstToken(staffUser);
                if (!string.IsNullOrWhiteSpace(firstStaff))
                    return UnifiedPrefix + firstStaff;

                return DefaultFolder;
            }

            // "normal" case: use module CRN as folder name by convention
            if (!string.IsNullOrWhiteSpace(firstCrn))
                return firstCrn;

            return DefaultFolder;
        }

        private static string ResolveOwner(string? staffUsernames)
        {
            var first = FirstToken(staffUsernames);
            if (string.IsNullOrWhiteSpace(first))
                return SchedulerAccount;

            return UnifiedPrefix + first;
        }
    }
}
