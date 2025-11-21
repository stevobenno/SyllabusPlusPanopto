using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.Extensions.Options;
using SessionManagement;
using SyllabusPlusPanopto.Transform.Domain.Settings;
using SyllabusPlusPanopto.Transform.To_Sort;
using UsageReporting;
using AuthenticationInfo = UsageReporting.AuthenticationInfo;

namespace SyllabusPlusPanopto.Transform.ApiWrappers
{
    public interface IUsageManagementWrapper : IDisposable
    {
        SessionUsage IsSessionUsageOk(Session session, SessionFilter filter);
    }

    public class UsageManagementWrapper : IDisposable, IUsageManagementWrapper
    {
        private readonly UsageReportingClient _usageManager;
        private readonly AuthenticationInfo _authentication;
        private readonly PanoptoOptions _settings;

        public UsageManagementWrapper(
            IOptions<PanoptoOptions> panoptoOptions,
            IPanoptoBindingFactory bindingFactory)
        {
            _settings = panoptoOptions.Value;

            _authentication = new AuthenticationInfo
            {
                UserKey = _settings.Username,
                Password = _settings.Password
            };

            var binding = bindingFactory.CreateBinding();

            var endpoint = PanoptoEndpointBuilder.BuildEndpoint(
                _settings.BaseUrl,
                _settings.UsageReportingPath); // add this to PanoptoSettings if it isn't there

            _usageManager = new UsageReportingClient(binding, endpoint);

            CertificateValidation.EnsureCertificateValidation();
        }

        public SessionUsage IsSessionUsageOk(Session session, SessionFilter filter)
        {
            var sessUsage = new SessionUsage();

            if (session?.StartTime == null)
                return sessUsage;

            IEnumerable<SummaryUsageResponseItem> response = _usageManager.GetSessionSummaryUsage(
                _authentication,
                session.Id,
                session.StartTime.Value,
                DateTime.UtcNow,
                UsageGranularity.Hourly);

            var summary = response?.FirstOrDefault();
            if (summary == null)
                return sessUsage;

            sessUsage.MinutesViewed = summary.MinutesViewed;
            sessUsage.NumberOfViews = summary.Views;
            sessUsage.NumberOfVisitors = summary.UniqueUsers;

            sessUsage.IsOk =
                summary.MinutesViewed >= filter.MinutesViewed &&
                summary.Views >= filter.NumberOfViews &&
                summary.UniqueUsers >= filter.NumberOfVisitors;

            return sessUsage;
        }

        public void Dispose()
        {
            try
            {
                if (_usageManager.State == CommunicationState.Faulted)
                {
                    _usageManager.Abort();
                }
                else if (_usageManager.State != CommunicationState.Closed)
                {
                    _usageManager.Close();
                }
            }
            catch
            {
                _usageManager.Abort();
            }
        }
    }
}
