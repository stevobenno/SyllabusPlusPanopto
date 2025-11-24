using System;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public sealed class SoapPanoptoPlatform : IPanoptoPlatform, IDisposable
    {
        public IRecorderApi Recorders { get; }
        public ISessionApi Sessions { get; }
        public IFolderApi Folders { get; }

        private readonly ISessionManagementWrapper _sessionsWrapper;
        private readonly IRemoteRecorderManagementWrapper _recordersWrapper;

        public SoapPanoptoPlatform(
            ISessionManagementWrapper sessionsWrapper,
            IRemoteRecorderManagementWrapper recordersWrapper,
            ISessionApi sessionApi,
            IRecorderApi recorderApi,
            IFolderApi folderApi = null)
        {
            _sessionsWrapper = sessionsWrapper
                               ?? throw new ArgumentNullException(nameof(sessionsWrapper));

            _recordersWrapper = recordersWrapper
                                ?? throw new ArgumentNullException(nameof(recordersWrapper));

            Sessions = sessionApi
                       ?? throw new ArgumentNullException(nameof(sessionApi));

            Recorders = recorderApi
                        ?? throw new ArgumentNullException(nameof(recorderApi));

            Folders = folderApi; // optional interface
        }

        public void Dispose()
        {
            (_sessionsWrapper as IDisposable)?.Dispose();
            (_recordersWrapper as IDisposable)?.Dispose();
        }
    }
}
