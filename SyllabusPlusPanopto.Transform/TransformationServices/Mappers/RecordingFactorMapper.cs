namespace SyllabusPlusPanopto.Integration.TransformationServices.Mappers
{
    /// <summary>
    /// S+ "Factor" values from the sheet:
    /// 1 – record with slides, audio and camera
    /// 2 – record with slides and audio only
    /// 3 – do not record
    /// 4 – as for 1 but also broadcast
    /// 5 – as for 2 but also broadcast
    /// Panopto CSV wants: Webcast = 1 or 0.
    /// Therefore: 4 or 5 → 1, else 0.
    /// </summary>
    internal static class RecordingFactorMapper
    {
        public static int ToWebcastFlag(int factor)
        {
            return factor == 4 || factor == 5 ? 1 : 0;
        }
    }
}
