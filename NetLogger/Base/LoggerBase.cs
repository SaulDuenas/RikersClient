using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLogger.Base
{
    public abstract class LoggerBase : ILogger
    {
        // private List<uData> _EventLogSelect = new List<uData>();

        public string strEventLogSelect { get { return strEventLogSelect; 
                                              } 
                                          set { var strEventLogSelect = value;
                                                this.SetEventList(strEventLogSelect);
                                              } 
                                        }

        public LoggerBase()
        {
            strEventLogSelect = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Logger"]) ? "Information,Warning,Error,SuccessAudit,FailureAudit" : ConfigurationManager.AppSettings["Logger"];
        }

        public List<uData> EventLogSelect {  get; set;  }

        static public List<uData> EventLogList = new List<uData>() { new uData("Information", EventLogEntryType.Information),
                                                                     new uData("Warning", EventLogEntryType.Warning),
                                                                     new uData("Error", EventLogEntryType.Error),
                                                                     new uData("SuccessAudit", EventLogEntryType.SuccessAudit),
                                                                     new uData("FailureAudit", EventLogEntryType.FailureAudit)
                                                                   };


        private void SetEventList(string list)  {

            List<string> eventlist = new List<string>(list.Split(',').Select(s => s.Trim()).ToArray());

            this.EventLogSelect = (from item in EventLogList
                                    join elist in eventlist on item.Label equals elist.Trim()
                                    select new uData(item.Label, item.Value)).ToList();

        }

        public bool isEventTypeSelect(EventLogEntryType type)
        {
            return this.EventLogSelect.Where(p => (EventLogEntryType)p.Value == type).Any();
        }


        public bool isEventTypeSelect(string type)
        {
            return this.EventLogSelect.Where(p => p.Label == type).Any();

        }

        public void Info(string source, string message, int eventID)
        {
            this.WriteLog(source, EventLogEntryType.Information, message, eventID);
        }

        public void Warning(string source, string message, int eventID)
        {
            this.WriteLog(source, EventLogEntryType.Warning, message, eventID);
        }
        public void Error(string source, string message, int eventID)
        {
            this.WriteLog(source, EventLogEntryType.Error, message, eventID);
        }
        public void SuccessAudit(string source, string message, int eventID)
        {
            this.WriteLog(source, EventLogEntryType.SuccessAudit, message, eventID);
        }
        public void FailureAudit(string source, string message, int eventID)
        {
            this.WriteLog(source, EventLogEntryType.FailureAudit, message, eventID);
        }

        public virtual void WriteLog(string source, EventLogEntryType type, string message, int eventID)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteLog(string source, string type, string message, int eventID)
        {
            throw new NotImplementedException();
        }
    }

    public class uData
    {
        private string _label;
        private object _value;



        public string Label { get => _label; set => _label = value; }
        public object Value { get => _value; set => this._value = value; }

        public uData()
        {
        }

        public uData(string label, object value)
        {
            _label = label;
            _value = value;
        }

    }
}
