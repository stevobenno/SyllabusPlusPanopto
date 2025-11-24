using Microsoft.Extensions.Logging;
using RemoteRecorderManagement;
using SessionManagement;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.Interfaces.PanoptoPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SyllabusPlusPanopto.Integration.To_Sort
{
    public class SoapSessionApi : ISessionApi
    {
        private readonly ISessionManagementWrapper _sm;
        private readonly IRemoteRecorderManagementWrapper _rrm;
        private readonly ILogger<PanoptoSyncService> _logger;

        public SoapSessionApi(
            ISessionManagementWrapper sm,
            IRemoteRecorderManagementWrapper rrm, ILogger<PanoptoSyncService> logger)
        {
            _sm = sm ?? throw new ArgumentNullException(nameof(sm));
            _rrm = rrm ?? throw new ArgumentNullException(nameof(rrm));
            _logger = logger;
        }

        public Task<ScheduleResult> ScheduleAsync(
            string title,
            Guid folderId,
            bool webcast,
            DateTime startUtc,
            DateTime endUtc,
            IEnumerable<Guid> recorderIds,
            bool overwrite,
            CancellationToken ct)
        {
            var recSettings = (recorderIds ?? Enumerable.Empty<Guid>())
                .Select(id => new RecorderSettings { RecorderId = id })
                .ToArray();

            var res = _rrm.ScheduleRecording(
                title,
                folderId,
                webcast,
                startUtc,
                endUtc,
                recSettings,
                overwrite);

            var log = res?.LogLine ?? res?.Result ?? string.Empty;
            var ok = res?.Success == true;
            Guid? sid = (res != null && res.SessionId != Guid.Empty)
                ? res.SessionId
                : null;

            return Task.FromResult(
                new ScheduleResult(ok, sid, Array.Empty<Guid>(), log));
        }

        public Task SetOwnerAsync(Guid sessionId, string owner, CancellationToken ct)
        {
            // TODO: implement when ownership needs to be driven via API
            return Task.CompletedTask;
        }

        public Task SetExternalIdAsync(Guid sessionId, string externalId, CancellationToken ct)
        {
            _sm.UpdateSessionExternalId(sessionId, externalId);
            return Task.CompletedTask;
        }

        public Task SetAvailabilityStartAsync(Guid sessionId, DateTime startUtc, CancellationToken ct)
        {
            // TODO: implement if/when availability is managed from here
            return Task.CompletedTask;
        }

        public Task DeleteAsync(IEnumerable<Guid> sessionIds, CancellationToken ct)
        {
            _sm.DeleteSessions((sessionIds ?? Array.Empty<Guid>()).ToArray());
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ExistingSession>> ListScheduledAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct)
        {
            var arr = _sm.GetSessionsInDateRange(fromUtc, toUtc)
                      ?? Array.Empty<Session>();

            var list = new List<ExistingSession>(arr.Length);

            foreach (var s in arr)
            {
                var externalId = _sm.GetExternalId(s.Id) ?? string.Empty;
                var start = s.StartTime ?? DateTime.MinValue;
                if (s.Duration != null)
                {
                    var end = start.AddSeconds(s.Duration.Value);

                    list.Add(new ExistingSession(
                        s.Id,
                        externalId,
                        start,
                        end));
                }
                else
                {
                    _logger.LogWarning(
                        "Panopto session {SessionId} has null Duration. Start={Start}, ExternalId={ExternalId}",
                        s.Id,
                        start,
                        externalId
                    );
                }

            }

            return Task.FromResult<IReadOnlyList<ExistingSession>>(list);
        }
    }
}
