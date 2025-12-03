using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SessionManagement;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces.ApiWrappers;
using SyllabusPlusPanopto.Integration.To_Sort;

namespace SyllabusPlusPanopto.Integration.ApiWrappers
{
    public class SessionManagementWrapper : IDisposable, ISessionManagementWrapper
    {
        private const int ResultsPerPage = 250; // Max is 10,000

        private readonly AuthenticationInfo _authentication;

        private readonly IOptions<PanoptoOptions> _panoptoOptions;
        private readonly IPanoptoBindingFactory _bindingFactory;
        private readonly SessionManagementClient _sessionManager;
        private readonly PanoptoOptions _settings;

        private readonly Dictionary<string, Folder> _folderByNameCache;
        private readonly Dictionary<Guid, Folder> _folderByIdCache;
        public SessionManagementWrapper(
            IOptions<PanoptoOptions> panoptoOptions,
            IPanoptoBindingFactory bindingFactory)
        {
            _panoptoOptions = panoptoOptions;
            _bindingFactory = bindingFactory;
            _settings = panoptoOptions.Value;

            _authentication = new AuthenticationInfo
            {
                UserKey = _settings.Username,
                Password = _settings.Password
            };

            var binding = _bindingFactory.CreateBinding();

            var endpoint = PanoptoEndpointBuilder.BuildEndpoint(
                _settings.BaseUrl,
                _settings.SessionManagementPath);

            _sessionManager = new SessionManagementClient(binding, endpoint);

            CertificateValidation.EnsureCertificateValidation();

            _folderByNameCache = new Dictionary<string, Folder>();
            _folderByIdCache = new Dictionary<Guid, Folder>();
        }

        // ----------------------- Folder helpers -----------------------

        public Folder GetFolderByName(string folderName)
        {
            var key = folderName.ToLowerInvariant();

            if (!_folderByNameCache.ContainsKey(key))
            {
                var result = GetChosenFolder(folderName);
                _folderByNameCache[key] = result;

                if (result != null)
                {
                    _folderByIdCache[result.Id] = result;
                }
            }

            return _folderByNameCache[key];
        }


        public void UpdateSessionOwner(Guid[] sessionIds, string ownerUserKey)
        {
            if (sessionIds == null || sessionIds.Length == 0) return;
            if (string.IsNullOrWhiteSpace(ownerUserKey)) return;

            _sessionManager.UpdateSessionOwner(
                _authentication,
                sessionIds,
                ownerUserKey);
        }

        public void UpdateSessionsAvailabilityStart(Guid[] sessionIds, DateTime availabilityUtc)
        {
            if (sessionIds == null || sessionIds.Length == 0) return;

            _sessionManager.UpdateSessionsAvailabilityStartSettings(
                _authentication,
                sessionIds,
                SessionStartSettingType.SpecificDate,
                availabilityUtc);
        }


        public Folder GetFolderByQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var key = query.ToLowerInvariant();

            // Return from cache if discovered previously
            if (_folderByNameCache.TryGetValue(key, out var cached))
                return cached;

            // 1. Exact match
            var allFolders = GetAllMatchingFolders(query);


            var exact = allFolders.FirstOrDefault(f =>
                    string.Equals(f.Name, query, StringComparison.InvariantCultureIgnoreCase));

            if (exact != null)
                return CacheAndReturn(key, exact);

            // 2. Substring match – the "real" CRN resolution
            var contains = allFolders
                .Where(f => f.Name.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0)
                .ToList();

            if (contains.Count == 1)
                return CacheAndReturn(key, contains[0]);

            if (contains.Count > 1)
            {
                // Best-guess match rule:
                // Prefer folder names *starting* with the query (CRN-style)
                var starts = contains
                    .Where(f => f.Name.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                if (starts.Count == 1)
                    return CacheAndReturn(key, starts[0]);

                // Fall back to first sorted by length → most specific first
                var chosen = contains
                    .OrderBy(f => f.Name.Length)
                    .First();

                return CacheAndReturn(key, chosen);
            }

            // 3. No match
            CacheAndReturn(key, null);
            return null;
        }

        private Folder CacheAndReturn(string key, Folder f)
        {
            _folderByNameCache[key] = f;
            if (f != null)
                _folderByIdCache[f.Id] = f;
            return f;
        }

        // GET ALL FOLDERS (cached across run)
        private List<Folder> _allFolders;
        private List<Folder> GetAllFolders()
        {
            if (_allFolders != null)
                return _allFolders;

            var results = new List<Folder>();

            for (var page = 0; page < 1000; page++)
            {
                var pagination = new Pagination
                {
                    MaxNumberResults = ResultsPerPage,
                    PageNumber = page
                };

                var response = _sessionManager.GetFoldersList(
                    _authentication,
                    new ListFoldersRequest
                    {
                        Pagination = pagination,
                        WildcardSearchNameOnly = false  // <-- FULL listing, not filtered
                    },
                    null);

                results.AddRange(response.Results);

                if (results.Count >= response.TotalNumberResults)
                    break;
            }

            _allFolders = results;
            return _allFolders;
        }


        private Folder GetChosenFolder(string folderName)
        {
            Folder result = null;

            if (_folderByNameCache.ContainsKey(folderName))
            {
                result = _folderByNameCache[folderName];
            }
            else
            {
                var matchingFolders = GetAllMatchingFolders(folderName);
                if (matchingFolders.Count > 1)
                {
                    // original UI removed — return first match
                    result = matchingFolders[0];
                }
                else
                {
                    result = matchingFolders.SingleOrDefault();
                }
            }

            return result;
        }

        private List<Folder> GetAllMatchingFolders(string folderName)
        {
            var result = new List<Folder>();

            for (var page = 0; page < 100; page++)
            {
                var pagination = new Pagination { MaxNumberResults = ResultsPerPage, PageNumber = page };
                var response = _sessionManager.GetFoldersList(
                    _authentication,
                    new ListFoldersRequest
                    {
                        Pagination = pagination,
                        WildcardSearchNameOnly = true,
                    },
                    folderName);

                result.AddRange(response.Results);

                if (result.Count >= response.TotalNumberResults)
                    break;
            }

            return result
                .Where(f => string.Equals(f.Name, folderName, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }

        public Folder GetFolderById(Guid id)
        {
            if (!_folderByIdCache.ContainsKey(id))
            {
                _folderByIdCache[id] = _sessionManager
                    .GetFoldersById(_authentication, new[] { id })
                    .Single();
            }
            return _folderByIdCache[id];
        }

        private string[] GetFullFolderStrings(List<Folder> matchingFolders)
        {
            var folderStrings = new string[matchingFolders.Count];

            for (var i = 0; i < matchingFolders.Count; ++i)
            {
                var currentParent = matchingFolders[i];
                var folderPath = currentParent.Name;

                while (currentParent.ParentFolder.HasValue)
                {
                    currentParent = GetFolderById(currentParent.ParentFolder.Value);
                    folderPath = currentParent.Name + "/" + folderPath;
                }

                folderStrings[i] = folderPath;
            }

            return folderStrings;
        }

        // ----------------------- Sessions: listing -----------------------

        public Session[] GetSessionsInDateRange(DateTime start, DateTime end)
        {
            Session[] result = null;

            var pageNum = 0;
            var firstRun = true;
            var resultIndex = 0;

            int totalItem;
            int itemRead = 0;

            do
            {
                var pagination = new Pagination { MaxNumberResults = 10, PageNumber = pageNum };
                var response = _sessionManager.GetSessionsList(
                    _authentication,
                    new ListSessionsRequest
                    {
                        Pagination = pagination,
                        StartDate = start,
                        EndDate = end
                    },
                    null);

                if (response == null)
                    throw new Exception(
                        $"Unable to fetch sessions between dates {start} and {end}");

                totalItem = response.TotalNumberResults;
                itemRead += 10;
                pageNum++;

                if (firstRun)
                {
                    firstRun = false;
                    result = new Session[totalItem];
                }

                foreach (var session in response.Results)
                {
                    result[resultIndex++] = session;
                }

            } while (itemRead < totalItem);

            return result;
        }

        public Session[] GetSessionById(Guid id)
        {
            return _sessionManager.GetSessionsById(_authentication, new[] { id });
        }

        public bool TryGetSessionId(string sessionName, out Guid sessionId)
        {
            sessionId = Guid.Empty;

            var pagination = new Pagination { MaxNumberResults = 25, PageNumber = 0 };
            var sessions = _sessionManager.GetSessionsList(
                _authentication,
                new ListSessionsRequest { Pagination = pagination },
                null);

            var session = sessions.Results.SingleOrDefault(s => s.Name == sessionName);
            if (session != null)
            {
                sessionId = session.Id;
                return true;
            }
            return false;
        }

        // ----------------------- ExternalId (updated metadata model) -----------------------

        public string GetExternalId(Guid sessionId)
        {
            var sessions = _sessionManager.GetSessionsById(_authentication, new[] { sessionId });
            var s = (sessions ?? Array.Empty<Session>()).SingleOrDefault();
            return s?.ExternalId ?? string.Empty;
        }

        public void UpdateSessionExternalId(Guid id, string externalId)
        {
            _sessionManager.UpdateSessionExternalId(_authentication, id, externalId);
        }

        // Backwards compatibility
        public string GetDescription(Guid sessionId) => GetExternalId(sessionId);
        public void UpdateSessionDescription(Guid id, string desc) =>
            UpdateSessionExternalId(id, desc);

        public bool RemoveProcessedMarker(Guid sessionId, string flagToken)
        {
            var s = GetSessionById(sessionId)?.FirstOrDefault();
            if (s == null) return false;

            var current = s.ExternalId ?? "";
            var token = Regex.Escape(string.IsNullOrEmpty(flagToken) ? "#ProcessingCompleted:" : flagToken);
            var pattern = token + @"[A-Za-z0-9:_\-\.\+TZ]*#?";

            var updated = Regex.Replace(current, pattern, "", RegexOptions.IgnoreCase).Trim();
            updated = Regex.Replace(updated, @"\s{2,}", " ");

            if (updated == current) return false;

            UpdateSessionExternalId(sessionId, updated);
            return true;
        }

        public bool AppendProcessedMarker(Guid sessionId, string markerPrefix, string timestampFormat = "o")
        {
            var current = GetExternalId(sessionId);

            if (!string.IsNullOrEmpty(current) &&
                current.IndexOf(markerPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            var stamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
            var combined = $"{markerPrefix}{stamp}";

            UpdateSessionExternalId(sessionId, combined);
            return true;
        }

        // ----------------------- Delete -----------------------

        public bool DeleteSessions(Guid[] sessionIds)
        {
            try
            {
                _sessionManager.DeleteSessions(_authentication, sessionIds);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ----------------------- Misc -----------------------

        public bool IsOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            if (start1 >= end2) return false;
            if (end1 <= start2) return false;
            return true;
        }

        public void Dispose()
        {
            if (_sessionManager == null) return;

            try
            {
                if (_sessionManager.State == CommunicationState.Faulted)
                {
                    _sessionManager.Abort();
                    return;
                }

                if (_sessionManager.State != CommunicationState.Closed)
                {
                    _sessionManager.Close();
                }
            }
            catch
            {
                _sessionManager.Abort();
            }
        }
    }
}
