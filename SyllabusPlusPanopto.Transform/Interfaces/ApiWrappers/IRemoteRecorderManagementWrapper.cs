using System;
using RemoteRecorderManagement;
using SyllabusPlusPanopto.Transform.ApiWrappers;

namespace SyllabusPlusPanopto.Transform.Interfaces.ApiWrappers;

public interface IRemoteRecorderManagementWrapper
{
    RecorderSettings GetSettingsByRecorderName(string name);
    Guid[] GetSessionsByRecorderName(string name);
    SchedulingResult ScheduleRecording(string name, Guid folderId, bool isBroadcast, DateTime startTime, DateTime endTime, RecorderSettings[] settings, bool overwrite);
    void Dispose();
    LoginResults GetListRecordersForLoginVerification();
}
