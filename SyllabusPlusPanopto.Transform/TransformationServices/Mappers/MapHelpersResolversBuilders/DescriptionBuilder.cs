namespace SyllabusPlusPanopto.Integration.TransformationServices.Mappers.MapHelpersResolversBuilders
{
    /// <summary>
    /// Reproduces the description logic from the spreadsheet:
    /// Base text:
    ///   "The full name of this activity is: " + ActivityName
    /// Then either:
    ///   ". No Presenter name has been provided"
    /// or:
    ///   ". The presenter(s) named for this event are: " + StaffName
    /// </summary>
    internal static class DescriptionBuilder
    {
        private const string ActivityPrefix = "The full name of this activity is: ";
        private const string NoPresenter = "No Presenter name has been provided";
        private const string PresenterPrefix = "The presenter(s) named for this event are: ";

        public static string Build(string activityName, string staffName)
        {
            var baseText = ActivityPrefix + (activityName ?? string.Empty);

            if (string.IsNullOrWhiteSpace(staffName))
                return baseText + ". " + NoPresenter;

            return baseText + ". " + PresenterPrefix + staffName;
        }
    }
}
