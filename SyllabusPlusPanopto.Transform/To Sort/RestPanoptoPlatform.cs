using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

/// <summary>
/// Placeholder REST-backed Panopto implementation.
/// Follows the same IPanoptoPlatform contract so the sync service
/// can switch to REST when Panopto’s public API reaches parity with SOAP.
/// </summary>
public sealed class RestPanoptoPlatform : IPanoptoPlatform
{
    public IRecorderApi Recorders { get; }
    public ISessionApi Sessions { get; }
    public IFolderApi Folders { get; }

    public RestPanoptoPlatform(IRestClient client)
    {
        Recorders = new RestRecorderApi(client);
        Sessions = new RestSessionApi(client);
        Folders = new RestFolderApi(client);
    }
}
