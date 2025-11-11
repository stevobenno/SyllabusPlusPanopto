using System;

namespace SyllabusPlusPanopto.Transform.TransformationServices.Mappers.MapHelpersResolversBuilders
{
    /// <summary>
    /// Mirrors the time adjustments in the Excel workbook:
    /// - Start = S+ start + 2 minutes
    /// - End = S+ end - 2 minutes
    /// - Workbook currently assumes 0 offset to UTC.
    /// We centralise it here so the Automapper profile stays clean.
    /// </summary>
    internal static class TimeHelper
    {
        // these two come from the Variables sheet in the workbook
        private static readonly TimeSpan StartOffset = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan EndOffset = TimeSpan.FromMinutes(2);

        // if later we need to apply UK→UTC (BST/GMT) logic, we do it here
        private static readonly TimeSpan UtcDiff = TimeSpan.Zero;

        public static DateTime ToUtcWithStartOffset(DateTime startDate, TimeSpan startTime)
        {
            // build local datetime first
            var local = startDate.Date + startTime + StartOffset + UtcDiff;
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);
        }

        public static DateTime ToUtcWithEndOffset(DateTime startDate, TimeSpan endTime)
        {
            var local = startDate.Date + endTime - EndOffset + UtcDiff;
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);
        }
    }
}
