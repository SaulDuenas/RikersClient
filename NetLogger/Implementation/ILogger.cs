using NetLogger.Base;
using System.Collections.Generic;
using System.Diagnostics;

namespace NetLogger.Implementation
{
    public interface ILogger
    {
        List<LoggerBase> LogAppender { get; set; }

        void Error(string source, string message, int eventID);
        void FailureAudit(string source, string message, int eventID);
        void Info(string source, string message, int eventID);
        void SuccessAudit(string source, string message, int eventID);
        void Warning(string source, string message, int eventID);
        void WriteLog(string source, EventLogEntryType type, string message, int eventID);
        void WriteLog(string source, string type, string message, int eventID);
    }
}