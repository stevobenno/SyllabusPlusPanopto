using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RemoteRecorderManagement;
using SessionManagement;
using SyllabusPlusPanopto.Integration.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;

namespace SyllabusPlusPanopto.Integration.To_Sort;

internal sealed class SoapSessionApi : ISessionApi
{
    private readonly SessionManagementWrapper _sm;
    private readonly RemoteRecorderManagementWrapper _rrm;
    private readonly string _user;
    private readonly string _pass;

    public SoapSessionApi(SessionManagementWrapper sm, RemoteRecorderManagementWrapper rrm, string user, string pass)
    {
        _sm = sm;
        _rrm = rrm;
        _user = user;
        _pass = pass;
    }

    public Task<ScheduleResult> ScheduleAsync(
        string title, Guid folderId, bool webcast,
        DateTime startUtc, DateTime endUtc,
        IEnumerable<Guid> recorderIds, bool overwrite, CancellationToken ct)
    {
        var recSettings = recorderIds.Select(id => new RecorderSettings { RecorderId = id }).ToArray();
        var res = _rrm.ScheduleRecording(title, folderId, webcast, startUtc, endUtc, recSettings, overwrite);

        var log = res?.LogLine ?? res?.Result ?? string.Empty;
        var ok = res?.Success == true;
        Guid? sid = (res != null && res.SessionId != Guid.Empty) ? res.SessionId : null;

        return Task.FromResult(new ScheduleResult(ok, sid, Array.Empty<Guid>(), log));
    }

    public Task SetOwnerAsync(Guid sessionId, string owner, CancellationToken ct)
    {
        // TODO
        //_sm.(_sm.Authentication, new[] { sessionId }, owner);
        return Task.CompletedTask;
    }

    public Task SetExternalIdAsync(Guid sessionId, string externalId, CancellationToken ct)
    {
        _sm.UpdateSessionExternalId(sessionId, externalId);
        return Task.CompletedTask;
    }

    public Task SetAvailabilityStartAsync(Guid sessionId, DateTime startUtc, CancellationToken ct)
    {
        //_sm.UpdateSessionsAvailabilityStartSettings(
        //    _sm.Authentication,
        //    new[] { sessionId },
        //    PanoptoScheduleUploader.Services.SessionManagement.SessionStartSettingType.SpecificDate,
        //    startUtc);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(IEnumerable<Guid> sessionIds, CancellationToken ct)
    {
        _sm.DeleteSessions(sessionIds.ToArray());
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ExistingSession>> ListScheduledAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        var arr = _sm.GetSessionsInDateRange(fromUtc, toUtc) ?? Array.Empty<SessionManagement.Session>();
        var list = new List<ExistingSession>(arr.Length);

        foreach (Session s in arr)
        {
            //// ExternalId helper is in wrapper
            //var externalId = _sm.GetExternalId(s.Id) ?? string.Empty;
            //list.Add(new ExistingSession(s.Id, externalId, s.StartTime.Value, s. }

           
        }

        return Task.FromResult<IReadOnlyList<ExistingSession>>(list);
    }

}
