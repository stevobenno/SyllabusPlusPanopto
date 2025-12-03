using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Microsoft.Extensions.Options;
using RemoteRecorderManagement;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.To_Sort;
using AuthenticationInfo = RemoteRecorderManagement.AuthenticationInfo;
using Pagination = RemoteRecorderManagement.Pagination;

namespace SyllabusPlusPanopto.Integration.ApiWrappers
{
    public class RemoteRecorderManagementWrapper : IDisposable, IRemoteRecorderManagementWrapper
    {
        private readonly IOptions<PanoptoOptions> _panoptoOptions;
        private readonly IPanoptoBindingFactory _bindingFactory;
        private readonly RemoteRecorderManagementClient _remoteRecorderManager;

        private readonly AuthenticationInfo _authenticationInfo;
        private readonly string _dateTimeFormat;
        private Dictionary<string, RemoteRecorder> _recorders;
        private readonly object _lockRecorders = new object();
        private readonly PanoptoOptions _settings;

        // RemoteRecorderManagement.ListRecorders() (Gets the list of all recorders and allows filtering by recorder name.)
        // RemoteRecorderManagement.ScheduleRecording() (Creates a new recording on a particular remote recorder.)
        // RemoteRecorderManagement.UpdateRecordingTime() (Allows modification of a previously scheduled recording.)


        public RemoteRecorderManagementWrapper(
            IOptions<PanoptoOptions> panoptoOptions,
            IPanoptoBindingFactory bindingFactory)
        {
            _panoptoOptions = panoptoOptions;
            _bindingFactory = bindingFactory;
            _settings = panoptoOptions.Value;

            _authenticationInfo = new AuthenticationInfo
            {
                UserKey = _settings.Username,
                Password = _settings.Password
            };

            var binding = bindingFactory.CreateBinding();

            var endpoint = PanoptoEndpointBuilder.BuildEndpoint(
                _settings.BaseUrl,
                _settings.RemoteRecorderManagementPath);

            _remoteRecorderManager = new RemoteRecorderManagementClient(binding, endpoint);

            CertificateValidation.EnsureCertificateValidation();
        }
        public RecorderSettings GetSettingsByRecorderName(string name)
        {
            try
            {
                EnsureRecorders();
                if (!_recorders.TryGetValue(name, out var recorder))
                {
                    return null;
                }

                return new RecorderSettings { RecorderId = recorder.Id };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Guid[] GetSessionsByRecorderName(string name)
        {
            EnsureRecorders();
            if (!_recorders.TryGetValue(name, out var recorder))
            {
                return null;
            }

            return recorder.ScheduledRecordings.ToArray();
        }

        private void EnsureRecorders()
        {
            lock (_lockRecorders)
            {
                if (_recorders == null)
                {
                    _recorders = new Dictionary<string, RemoteRecorder>();

                    var retryCount = 0;
                    for (var pageNumber = 0; ; pageNumber++)
                    {
                        try
                        {
                            var response = _remoteRecorderManager.ListRecorders(
                                _authenticationInfo,
                                new Pagination
                                {
                                    MaxNumberResults = 30,
                                    PageNumber = pageNumber
                                },
                                RecorderSortField.Name
                            );

                            if (response.PagedResults.Length > 0)
                            {
                                foreach (var recorder in response.PagedResults)
                                {
                                    _recorders[recorder.Name] = recorder;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (ProtocolException e)
                        {
                            Trace.WriteLine($"ListRecorders threw: {e}");
                            retryCount++;
                            if (retryCount > 10)
                            {
                                break;
                            }

                            Trace.WriteLine($"Retrying {retryCount} times.");
                            pageNumber--;
                        }
                    }
                }
            }
        }

        public SchedulingResult ScheduleRecording(string name, Guid folderId, bool isBroadcast, DateTime startTime, DateTime endTime, RecorderSettings[] settings, bool overwrite)
        {
            if (folderId.Equals(Guid.Empty)) return new SchedulingResult(false, $"Recording {name} failed; folder not found.", Guid.Empty);
            ScheduledRecordingResult result = null;
            var conflictingSessions = "";
            var retry = true;
            var attemptCount = 0;
            while (retry)
            {
                try
                {
                    result = _remoteRecorderManager.ScheduleRecording(_authenticationInfo, name, folderId, isBroadcast, startTime.ToUniversalTime(), endTime.ToUniversalTime(), settings);
                    retry = false;

                    if (result.ConflictsExist)
                    {

                        if (overwrite == true)
                        {
                            using (var sessionManager = new SessionManagementWrapper(_panoptoOptions, _bindingFactory))
                            {
                                foreach (var session in result.ConflictingSessions)
                                {
                                    var sessiondeleted = sessionManager.DeleteSessions(new Guid[] { session.SessionID });
                                    if (sessiondeleted == false)
                                    {
                                        return new SchedulingResult(false, string.Format("Unable to schedule recording {0} between {1} and {2} due to schedule conflicts. Unable to delete session {3}",
                                            name, startTime.ToString(_dateTimeFormat), endTime.ToString(_dateTimeFormat), session.SessionID), Guid.Empty);
                                    }

                                    conflictingSessions += string.Format("{0} ", session.SessionID);
                                }

                            }
                            result = _remoteRecorderManager.ScheduleRecording(_authenticationInfo, name, folderId, isBroadcast, startTime.ToUniversalTime(), endTime.ToUniversalTime(), settings);
                            return new SchedulingResult(true, string.Format("Recording {0} was scheduled between {1} and {2} overwriting previously scheduled sessions",
                                name, startTime.ToString(_dateTimeFormat), endTime.ToString(_dateTimeFormat)), result.SessionIDs[0], conflictingSessions);


                        }

                        return new SchedulingResult(false, string.Format("Unable to schedule recording {0} between {1} and {2} due to schedule conflicts.",
                            name, startTime.ToString(_dateTimeFormat), endTime.ToString(_dateTimeFormat)), Guid.Empty, result.ConflictingSessions[0].SessionID.ToString());

                    }


                }
                catch (FaultException e)
                {
                    // only retry the call if it's possible this was a transient error (ie, a deadlock)
                    retry &= (e.Message == "An error occurred. See server logs for details") && (attemptCount < 3);
                    if (retry)
                    {
                        // sleep for a bit before retrying
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                    }
                    else
                    {
                        // if we're not retrying, re-throw the exception
                        throw;
                    }
                }
                attemptCount++;

            }

            return new SchedulingResult(true, string.Format("Recording {0} was scheduled between {1} and {2}",
                name, startTime.ToString(_dateTimeFormat), endTime.ToString(_dateTimeFormat)), result.SessionIDs[0]);

        }

        public void Dispose()
        {
            if (_remoteRecorderManager.State == CommunicationState.Faulted)
            {
                _remoteRecorderManager.Abort();
            }

            if (_remoteRecorderManager.State != CommunicationState.Closed)
            {
                _remoteRecorderManager.Close();
            }
        }

        public LoginResults GetListRecordersForLoginVerification()
        {
            try
            {
                var pagination = new Pagination { MaxNumberResults = 1, PageNumber = 0 };
                var recorderListResponse = _remoteRecorderManager.ListRecorders(_authenticationInfo, pagination, RecorderSortField.Name);

                // User has no Remote Recorder Access
                if (recorderListResponse.TotalResultCount < 1)
                    return LoginResults.NoAccess;
            }
            catch
            {
                return LoginResults.Failed;
            }
            return LoginResults.Succeeded;
        }
    }
}
