using System;
using SyllabusPlusPanopto.Transform.ApiWrappers;
using SyllabusPlusPanopto.Transform.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Transform.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Transform.To_Sort;

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
