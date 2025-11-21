namespace SyllabusPlusPanopto.Integration.TransformationServices.Mappers.MapHelpersResolversBuilders
{
    /// <summary>
    /// Excel rule (Rob):
    /// =IF(ISBLANK(RecorderFromSPlus),
    ///      XLOOKUP(LocationName, Variables!rooms, Variables!recorders, Variables!default),
    ///      RecorderFromSPlus)
    /// We don't have the Variables sheet in code, so we implement the precedence:
    /// 1. explicit recorder from S+
    /// 2. fall back to the location name (convention)
    /// 3. else "UNKNOWN_RECORDER"
    /// If/when AV give us a real room→recorder JSON, we swap the middle step.
    /// </summary>
    internal static class RecorderResolver
    {
        private const string UnknownRecorder = "UNKNOWN_RECORDER";

        public static string Resolve(string recorderFromSplus, string locationName)
        {
            if (!string.IsNullOrWhiteSpace(recorderFromSplus))
                return recorderFromSplus.Trim();

            if (!string.IsNullOrWhiteSpace(locationName))
                return locationName.Trim();

            return UnknownRecorder;
        }
    }
}
