using NetLogger.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetLogger.Implementation
{
    public class Winlogger: LoggerBase, ILogger
    {

        public ListBox ListBoxRef { get; set; }

        public Winlogger() { 
        }

        /*
        public Locallogger(ListBox listbox, string list) {

            ListBoxRef = listbox;

            List<string> eventlist = new List<string>(list.Split(',').Select(s => s.Trim()).ToArray());

            //  _EventLogSelect = EventLogList.Select(item => new uData(item.Label,item.Value)).Where(p => (EventLogEntryType)p.Value == type);

            this._EventLogSelect = (from item in EventLogList
                                    join elist in eventlist on item.Label equals elist.Trim()
                                    select new uData(item.Label, item.Value)).ToList();

        }

        */
        /*
        public Locallogger(ListBox listbox)
        {
            ListBoxRef = listbox;
        }
        */
      


        public override void WriteLog(string source, EventLogEntryType type, string message, int eventID)
        {
            if (isEventTypeSelect(type) && !ListBoxRef.Equals(null))
            {
                ListBoxRef.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Level : {type.ToString()} - Source: {source} - Description: {message}");
            }
        }

        public override void WriteLog(string source, string type, string message, int eventID) 
        {
            if (isEventTypeSelect(type) && !ListBoxRef.Equals(null))
            {
                ListBoxRef.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Level : {type.ToString()} - Source: {source} - Description: {message}");
            }

        } 

    }
}
