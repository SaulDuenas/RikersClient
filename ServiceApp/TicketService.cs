using NetLogger.Implementation;
using RikersProxy;
using Service.Domian;
using Service.Domian.Core;
using Service.Domian.Core.tickets;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Service.Domian.Core.Proxy;

namespace ServiceApp
{
    [RunInstaller(true)]
    internal partial class TicketService : ServiceBase
    {
       
        private int start_counter = 0;

        //  private EventViewertExt _eventExt = null;
       // private IClientProxy _proxyCli = null;

        private ILogger _logger = null;
        private FileLogger _filelog = null;

        private TicketServiceCore _ticketSrvCore = null;
        private FeedBackServiceCore _feedbackSrvCore = null;

        public TicketService()
        {
            InitializeComponent();

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("RikersInterface"))
            {
                EventLog.CreateEventSource("RikersInterface", "LogService");
            }
            eventLog1.Source = "RikersInterface";
            eventLog1.Log = "LogService";

        }

        protected override void OnStart(string[] args)
        {
   

            eventLog1.WriteEntry("In OnStart.");

            // TODO: Add code here to start your service.

          //  this._proxyCli = ClientProxy.Instance;
            this._filelog = new FileLogger();
            this._logger = new Logger();
            this._logger.LogAppender.Add(_filelog);

            if (startMonitoring()) {
                this._ticketSrvCore.run();
                this._feedbackSrvCore.run();
                
                this._logger.Info("Service", $"start monitoring - sprint {start_counter}", 100);

            }

            eventLog1.WriteEntry("End OnStart.");
        }

        protected override void OnStop()
        {
            try
            {
                eventLog1.WriteEntry("In OnStop.");
                this._logger.Info("Service", $"stop monitoring - sprint {start_counter}", 100);

                StopMonitoring();

                eventLog1.WriteEntry("End OnStop.");

                this._logger.Info("Service", $"Service Stoped", 100);

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("Service", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("Service", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("Service", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                throw;
            }
        }

        protected override void OnShutdown()
        {
            eventLog1.WriteEntry("In OnShutdown.");
        }



        private bool startMonitoring()
        {
            bool retval = false;

            start_counter++;

            // this._proxyCore = new ProxyCore(this._proxyCli, this._logger);
            string filter = ConfigurationManager.AppSettings["Filter"];

            string pathPendingTickets = ConfigurationManager.AppSettings["PathTicketsPending"];
          
            if (Directory.Exists(pathPendingTickets))
            {
                obsPathtickets.Path = pathPendingTickets;
                obsPathtickets.Filter = filter;
                obsPathtickets.EnableRaisingEvents = true;

                this._ticketSrvCore = new TicketServiceCore(this._logger);

                retval = true;
            }
            else
            {
                if (string.IsNullOrEmpty(pathPendingTickets)) this._logger.Error("Service", $"Parameter PathTicketsPending is Empty or not exist, check the parameter PathTicketsPending on App.Config.", 100);
                if (!Directory.Exists(pathPendingTickets)) this._logger.Error("Service", $"Path {pathPendingTickets} not exist", 100);

                retval = false;
            }

            string pathPendingFeedBack = ConfigurationManager.AppSettings["PathCommentsPending"];

            if (Directory.Exists(pathPendingFeedBack))
            {
                obsPathFeedBack.Path = pathPendingFeedBack;
                obsPathFeedBack.Filter = filter;
                obsPathFeedBack.EnableRaisingEvents = true;

                this._feedbackSrvCore = new  FeedBackServiceCore(this._logger);

                retval = true;
            }
            else
            {
                if (string.IsNullOrEmpty(pathPendingFeedBack)) this._logger.Error("Service", $"Parameter PathCommentsPending is Empty or not exist, check the parameter PathCommentsPending on App.Config.", 100);
                if (!Directory.Exists(pathPendingFeedBack)) this._logger.Error("Service", $"Path {pathPendingFeedBack} not exist", 100);

                retval = false;
            }
            
            return retval;
        }

        private bool StopMonitoring()
        {
            this._logger.Info("Service", $"stoping Task", 100);
            this._ticketSrvCore.stop();
            this._feedbackSrvCore.stop();

            this._logger.Info("Service", $"stoping Observer", 100);

            this._logger.SuccessAudit("Service", $"observer.EnableRaisingEvents = {obsPathtickets.EnableRaisingEvents}", 100);

            return false;

        }

        /* Eventos de identificacion de archivos de tickets */
        private void obsPathtickets_Created(object sender, FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            this._logger.Info("Service", $"file created, name: {file.Name} size: {file.Length} bytes.", 100);
            this._ticketSrvCore.RegisterFileTickettoCache(e.FullPath);
           
        }

        private void obsPathtickets_Changed(object sender, FileSystemEventArgs e)
        {
            var result = this._ticketSrvCore.UpdateFileTicketCache(e.FullPath);

            if (result) this._logger.Info("Service", $"Ticket file changed : {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

        }

        private void obsPathtickets_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void obsPathtickets_Renamed(object sender, RenamedEventArgs e)
        {

        }


        /* Eventos de identificacion de comentarios */
        private void obsPathFeedBack_Changed(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("Service", $"Comment file changed, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._feedbackSrvCore.UpdateFileCommentCache(e.FullPath);
        }

        private void obsPathFeedBack_Created(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("Service", $"Comment file created, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._feedbackSrvCore.RegisterFileFeedBacktoCache(e.FullPath);
        }

        private void obsPathFeedBack_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void obsPathFeedBack_Renamed(object sender, RenamedEventArgs e)
        {

        }



        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }

    }
}
