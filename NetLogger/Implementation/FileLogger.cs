
using NetLogger.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLogger.Implementation
{
    public class FileLogger : LoggerBase
    {
        public string FileName { get; set; }
        public string strPath { get; set; }

        public bool writeintoFile (string message)
        {
            try
            {
                if (!Directory.Exists(this.strPath) || string.IsNullOrEmpty(this.strPath))
                {
                    var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..");
                    this.strPath = string.Format("{0}\\{1}", appPath, "Log");

                    if (!Directory.Exists(this.strPath)) { Directory.CreateDirectory(this.strPath); }

                }

                this.FileName = string.IsNullOrEmpty(this.FileName) ? this.FileName = DateTime.Now.ToString("yyyyMMdd") + ".log" : FileName.Replace("yyyyMMdd", DateTime.Now.ToString("yyyyMMdd"));

                using (FileStream filestream = new FileStream(string.Format("{0}\\{1}", this.strPath, this.FileName), FileMode.Append, FileAccess.Write))
                {
                    StreamWriter stream = new StreamWriter((Stream)filestream);
                    stream.WriteLine(message);
                    stream.Close();
                    filestream.Close();
                }

                return true;


            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public override void WriteLog(string source, string type, string message, int eventID)
        {
            // base.WriteLog(source, type, message, eventID);

            if (isEventTypeSelect(type) )
            {
                this.writeintoFile($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Level : {type.ToString()} - Source: {source} - Description: {message}");
            }
 
        }

        public override void WriteLog(string source, EventLogEntryType type, string message, int eventID)
        {
            // base.WriteLog(source, type, message, eventID);

            if (isEventTypeSelect(type))
            {
                this.writeintoFile($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - Level : {type.ToString()} - Source: {source} - Description: {message}");
            }
        }


    }
}
