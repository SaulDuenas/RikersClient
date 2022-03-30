using NetLogger.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor.Directory
{
    public class EventViewertExt : LoggerBase, ILoggerBase
    {

        public EventViewertExt() {

            if (!EventLog.SourceExists("MySource"))

            {

                EventLog.CreateEventSource("MySource", "MyNewLog");

            }

        }
      

        public void Write(string source, EventLogEntryType type, string message, int eventID)
        {

            if (isEventTypeSelect(type))
            {
                EventLog eventlog = new EventLog();
                eventlog.Source = source;

                // Write an entry in the event log.
                eventlog.WriteEntry(message, type, eventID);
                eventlog.Dispose();
            }

        }

        public override void WriteLog(string source, EventLogEntryType type, string message, int eventID)
        {

            if (isEventTypeSelect(type))
            {

                EventLog eventlog = new EventLog();
                eventlog.Source = source;

                eventlog.WriteEntry(message, type, eventID);
                eventlog.Dispose();

            }

        }
    }
}
