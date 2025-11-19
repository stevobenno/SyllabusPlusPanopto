namespace SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform
{
    /// <summary>
    /// Root abstraction for Panopto operations (swappable SOAP/REST).
    /// </summary>
    public interface IPanoptoPlatform
    {
        IRecorderApi Recorders { get; }
        ISessionApi Sessions { get; }
        IFolderApi Folders { get; }
    }
}