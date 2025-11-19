using System;

namespace SyllabusPlusPanopto.Transform.TransformationServices.Mappers.MapHelpersResolversBuilders
{
    /// <summary>
    /// Mirrors the big LET() folder formula from the workbook, but in our
    /// convention-only world:
    /// - take first Module CRN (values are sometimes comma-separated)
    /// - if CRN starts with "#SPLUS" → it's a placeholder/special → send to staff personal
    ///   folder (unified\{first staff}) or else to default
    /// - else, return the CRN as the expected Panopto folder name
    /// - else, return default
    /// </summary>
    internal static class FolderNameBuilder
    {
        private const string DefaultFolder = "Recording Catchall";
        private const string UnifiedPrefix = "unified\\";

        public static string FromSyllabusPlus(string moduleCrn, string staffUser)
        {
            var firstCrn = FirstToken(moduleCrn);

            // workbook branch:
            // IF(LEFT(dVal, 6) = "#SPLUS") ...
            if (!string.IsNullOrWhiteSpace(firstCrn) &&
                firstCrn.StartsWith("#SPLUS", StringComparison.OrdinalIgnoreCase))
            {
                var firstStaff = FirstToken(staffUser);
                return string.IsNullOrWhiteSpace(firstStaff)
                    ? DefaultFolder
                    : UnifiedPrefix + firstStaff;
            }

            // normal case: CRN-driven folder, by convention
            if (!string.IsNullOrWhiteSpace(firstCrn))
                return firstCrn;

            return DefaultFolder;
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
