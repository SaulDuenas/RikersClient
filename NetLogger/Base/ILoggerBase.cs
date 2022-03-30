using System.Diagnostics;

namespace NetLogger.Base
{
    public interface ILoggerBase
    {
        void WriteLog(string source, EventLogEntryType type, string message, int eventID);
        void WriteLog(string source, string type, string message, int eventID);

    }
}