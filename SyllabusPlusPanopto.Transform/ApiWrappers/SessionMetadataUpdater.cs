//using System;
//using System.Globalization;
//using System.Linq;
//using SessionManagement;
//using SyllabusPlusPanopto.Transform.To_Sort;

//namespace SyllabusPlusPanopto.Transform.ApiWrappers
//{
//    /// <summary>
//    /// Thin helper around the 4.6 SessionManagement client for description metadata.
//    /// If you prefer, you can delete this file and use SessionManagementWrapper instead.
//    /// </summary>
//    public static class SessionMetadataUpdater
//    {
//        private static SessionManagementClient NewClient()
//        {
//            // Endpoint name must match app.config:
//            // <endpoint ... name="BasicHttpBinding_ISessionManagement" />
//            CertificateValidation.EnsureCertificateValidation();
//            return new SessionManagementClient("BasicHttpBinding_ISessionManagement");
//        }

//        private static AuthenticationInfo NewAuth(string username, string password) =>
//            new AuthenticationInfo { UserKey = username, Password = password };

//        public static string GetDescription(string username, string password, Guid sessionId)
//        {
//            using (var client = NewClient())
//            {
//                var auth = NewAuth(username, password);
//                var sessions = client.GetSessionsById(auth, new[] { sessionId });
//                var s = (sessions ?? Array.Empty<Session>()).FirstOrDefault();
//                return s?.Description ?? string.Empty;
//            }
//        }

//        public static void UpdateDescription(string username, string password, Guid sessionId, string description)
//        {
//            using (var client = NewClient())
//            {
//                var auth = NewAuth(username, password);
//                client.UpdateSessionDescription(auth, sessionId, description);
//            }
//        }

//        /// <summary>
//        /// Append a “processed” token unless already present. Returns true if we updated.
//        /// </summary>
//        public static bool AppendProcessedMarker(string username, string password, Guid sessionId,
//                                                 string markerPrefix, string timestampFormat = "o")
//        {
//            using (var client = NewClient())
//            {
//                var auth = NewAuth(username, password);

//                var sessions = client.GetSessionsById(auth, new[] { sessionId });
//                var s = (sessions ?? Array.Empty<Session>()).FirstOrDefault();
//                var current = s?.Description ?? string.Empty;

//                if (!string.IsNullOrEmpty(current) &&
//                    current.IndexOf(markerPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
//                {
//                    return false; // already marked
//                }

//                var token = $"{markerPrefix}{DateTime.UtcNow.ToString(timestampFormat, CultureInfo.InvariantCulture)}#";
//                var updated = string.IsNullOrWhiteSpace(current) ? token : (current.TrimEnd() + " " + token);

//                client.UpdateSessionDescription(auth, sessionId, updated);
//                return true;
//            }
//        }
//    }
//}
