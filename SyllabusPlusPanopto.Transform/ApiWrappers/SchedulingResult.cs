using System;

namespace SyllabusPlusPanopto.Integration.ApiWrappers
{
    public class SchedulingResult
    {
        public SchedulingResult(string result, Guid sessionId)
        {
            Success = true;
            Result = result;
            SessionId = sessionId;
        }

        public SchedulingResult(bool success, string result, Guid sessionId)
        {
            Success = success;
            Result = result;
            SessionId = sessionId;
        }

        public SchedulingResult(bool success, string result, Guid sessionId, string logline)
        {
            Success = success;
            Result = result;
            SessionId = sessionId;
            LogLine = logline;
        }

        public bool Success { get; set; }
        public string Result { get; set; }
        public Guid SessionId { get; set; }
        public string LogLine { get; set; }
    }
}
