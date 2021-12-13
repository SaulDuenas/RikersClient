using NetLogger.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLogger.Implementation
{
    public class Logger
    {
        public List<LoggerBase> LogAppender { get; set; }


        public Logger()
        {
            LogAppender = new List<LoggerBase>();

        }

        public void Info(string source, string message, int eventID)
        {
            if (LogAppender != null)
            {
                LogAppender.ForEach(item => item.WriteLog(source, EventLogEntryType.Information, message, eventID));

            }
        }

        public void Warning(string source, string message, int eventID)
        {
            if (LogAppender != null)
            {
                LogAppender.ForEach(item => item.WriteLog(source, EventLogEntryType.Warning, message, eventID));
            }
        }
        public void Error(string source, string message, int eventID)
        {
            if (LogAppender != null)
            {
                LogAppender.ForEach(item => item.WriteLog(source, EventLogEntryType.Error, message, eventID));

            }
        }
        public void SuccessAudit(string source, string message, int eventID)
        {
            if (LogAppender != null)
            {
                LogAppender.ForEach(item => item.WriteLog(source, EventLogEntryType.SuccessAudit, message, eventID));
            }
        }

        public void FailureAudit(string source, string message, int eventID)
        {

            if (LogAppender != null)
            {
                LogAppender.ForEach(item => item.WriteLog(source, EventLogEntryType.FailureAudit, message, eventID));

            }
        }


        public void WriteLog(string source, EventLogEntryType type, string message, int eventID)
        {
            if (LogAppender != null) {
                LogAppender.ForEach(item => item.WriteLog(source, type, message, eventID));
            }
        }

        public void WriteLog(string source, string type, string message, int eventID)
        {
            if (LogAppender != null)
            {
                LogAppender.ForEach(r => r.WriteLog(source, type, message, eventID));
            }
        }

    }
}
