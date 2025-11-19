using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Transform.To_Sort;
using UserManagement;

namespace SyllabusPlusPanopto.Transform.ApiWrappers
{
    public class UserManagementWrapper : IDisposable
    {
        private readonly UserManagementClient _userManager;
        private readonly AuthenticationInfo _authentication;
        private readonly Dictionary<Guid, User> _userByIdCache = new();
        private readonly PanoptoSettings _settings;
        private readonly Dictionary<string, User> _userByEmailCache;

        public UserManagementWrapper(
            IOptions<PanoptoSettings> panoptoOptions,
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
                _settings.UserManagementPath); // add this property to PanoptoSettings if it is not there yet

            _userManager = new UserManagementClient(binding, endpoint);

            CertificateValidation.EnsureCertificateValidation();

            _userByIdCache = new Dictionary<Guid, User>();
            _userByEmailCache = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
        }

        public User GetUserByUserName(string username)
        {
            return _userManager.GetUserByKey(_authentication, username);
        }

        public User GetUserById(Guid id)
        {
            if (id == Guid.Empty) return null;

            if (_userByIdCache.TryGetValue(id, out var cached))
                return cached;

            var users = _userManager.GetUsers(_authentication, new[] { id }) ?? Array.Empty<User>();
            var u = users.FirstOrDefault();
            if (u != null) _userByIdCache[id] = u;
            return u;
        }

        /// <summary>
        /// Returns the best-guess email for a CreatorId. Falls back from User.Email to UserKey→domain mapping.
        /// </summary>
        public string ResolveEmailFromCreatorId(Guid? creatorId, string fallbackDomain = "leeds.ac.uk")
        {
            if (!creatorId.HasValue || creatorId.Value == Guid.Empty) return string.Empty;

            var user = GetUserById(creatorId.Value);
            if (user == null) return string.Empty;

            if (!string.IsNullOrWhiteSpace(user.Email))
                return user.Email;

            // Fallback: many Panopto tenants store "unified\abc123" in UserKey
            var key = user.UserKey ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            if (key.Contains("@")) return key;

            const string unifiedPrefix = "unified\\";
            if (key.StartsWith(unifiedPrefix, StringComparison.OrdinalIgnoreCase))
                return key.Substring(unifiedPrefix.Length) + "@" + fallbackDomain;

            // last resort: return the key (UI can display it)
            return key;
        }

        public void Dispose()
        {
            if (_userManager == null) return;

            try
            {
                if (_userManager.State == CommunicationState.Faulted)
                {
                    _userManager.Abort();
                    return;
                }

                if (_userManager.State != CommunicationState.Closed)
                    _userManager.Close();
            }
            catch
            {
                _userManager.Abort();
            }
        }
    }
}
