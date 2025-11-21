using System;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

public sealed class SoapPanoptoPlatform : IPanoptoPlatform, IDisposable
{
    public IRecorderApi Recorders { get; }
    public ISessionApi Sessions { get; }
    public IFolderApi Folders { get; }

    private readonly SessionManagementWrapper _sessions;
    private readonly RemoteRecorderManagementWrapper _recorders;

    public SoapPanoptoPlatform(
        ISessionManagementWrapper sessions,
        IRemoteRecorderManagementWrapper recorders
       )
    {
       
    }

    public void Dispose()
    {
        _recorders?.Dispose();
        _sessions?.Dispose();
    }
}
