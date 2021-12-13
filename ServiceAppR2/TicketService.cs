using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ServiceAppR2
{
    partial class TicketService : ServiceBase
    {
        int ScheduleInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ThreadSleepTimeInMin"]);
        public Thread worker = null;

        public TicketService()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            try
            {
                ThreadStart start = new ThreadStart(Working);
                worker = new Thread(start);
                worker.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Working()
        {
            while (true)
            {

                string path = "C:\\Sample.txt";
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(string.Format("Windows Service Called on " + DateTime.Now.ToString("dd /MM/yyyy hh:mm:ss tt")));
                    writer.Close();
                }
                Thread.Sleep(ScheduleInterval * 60 * 1000);
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            try
            {
                if ((worker != null) & worker.IsAlive)
                {
                    worker.Abort();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
