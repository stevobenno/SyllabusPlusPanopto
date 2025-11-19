using System;

namespace SyllabusPlusPanopto.Transform.TransformationServices.Mappers.MapHelpersResolversBuilders
{
    /// <summary>
    /// Workbook rule:
    /// if StaffUserName is blank → scheduler@leeds.ac.uk
    /// else → take first staff username (before comma) and prepend "unified\"
    /// </summary>
    internal static class OwnerResolver
    {
        private const string SchedulerAccount = "scheduler@leeds.ac.uk";
        private const string UnifiedPrefix = "unified\\";

        public static string ResolveOwner(string staffUsernames)
        {
            var first = FirstToken(staffUsernames);
            if (string.IsNullOrWhiteSpace(first))
                return SchedulerAccount;

            return UnifiedPrefix + first;
        }

        private static string FirstToken(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var idx = input.IndexOf(',', StringComparison.Ordinal);
            return idx > 0 ? input[..idx].Trim() : input.Trim();
        }
    }
}
