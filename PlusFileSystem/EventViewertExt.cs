using Monitor.Directory;
using Service.Domian;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor.Directory
{
    public class EventViewertExt : ILogger
    {
        // private List<string> _Eventlist = new List<string>();

        private List<uData> _EventLogSelect = new List<uData>();

        static public List<uData> EventLogList = new List<uData>() { new uData("Information", EventLogEntryType.Information),
                                                                     new uData("Warning", EventLogEntryType.Warning),
                                                                     new uData("Error", EventLogEntryType.Error),
                                                                     new uData("SuccessAudit", EventLogEntryType.SuccessAudit),
                                                                     new uData("FailureAudit", EventLogEntryType.FailureAudit)
                                                                   };


        // Create an EventLog instance and assign its source.


        public EventViewertExt(string list)
        {

            List<string> eventlist = new List<string>(list.Split(',').Select(s => s.Trim()).ToArray());

            //  _EventLogSelect = EventLogList.Select(item => new uData(item.Label,item.Value)).Where(p => (EventLogEntryType)p.Value == type);

            this._EventLogSelect = (from item in EventLogList
                                    join elist in eventlist on item.Label equals elist.Trim()
                                    select new uData(item.Label, item.Value)).ToList();

            // Create the source and log, if it does not already exist.

            if (!EventLog.SourceExists("MySource"))

            {

                EventLog.CreateEventSource("MySource", "MyNewLog");

            }

        }

        public bool isEventTypeSelect(EventLogEntryType type)
        {
            return this._EventLogSelect.Where(p => (EventLogEntryType)p.Value == type).Any();
        }


        public bool isEventTypeSelect(string type)
        {
            return this._EventLogSelect.Where(p => p.Label == type).Any();

        }


        public void Info(string source, string message, int eventID)
        {
            this.Write(source, EventLogEntryType.Information, message, eventID);
        }

        public void Warning(string source, string message, int eventID)
        {
            this.Write(source, EventLogEntryType.Warning, message, eventID);
        }
        public void Error(string source, string message, int eventID)
        {
            this.Write(source, EventLogEntryType.Error, message, eventID);
        }
        public void SuccessAudit(string source, string message, int eventID)
        {
            this.Write(source, EventLogEntryType.SuccessAudit, message, eventID);
        }
        public void FailureAudit(string source, string message, int eventID)
        {
            this.Write(source, EventLogEntryType.FailureAudit, message, eventID);
        }


        public void Write(string source, EventLogEntryType type, string message, int eventID)
        {

            if (isEventTypeSelect(type))
            {
                EventLog eventLog = new EventLog();
                eventLog.Source = source;

                // Write an entry in the event log.
                eventLog.WriteEntry(message, type, eventID);
                eventLog.Dispose();
            }

        }



    }
}
