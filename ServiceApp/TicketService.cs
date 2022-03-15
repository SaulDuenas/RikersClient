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
using static Service.Domian.Core.TicketRepositoryCore;


namespace ServiceApp
{
    [RunInstaller(true)]
    internal partial class TicketService : ServiceBase
    {
        private bool BkgProccessIsOn = false;
      
        private int start_counter = 0;

        //  private EventViewertExt _eventExt = null;
        private IClientProxy _proxyCli = null;

        private Logger _logger = null;
        private FileLogger _filelog = null;

        private ProxyCore _proxyCore = null;
        private TicketServiceCore _ticketsrvcore = null;

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

            this._proxyCli = new ClientProxy();
            this._filelog = new FileLogger();
            this._logger = new Logger();

            if (startMonitoring()) {
                this._ticketsrvcore.run();
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
            
            this._logger.LogAppender.Add(_filelog);
            start_counter++;

            this._proxyCore = new ProxyCore(this._proxyCli, this._logger);

            string path_observer = ConfigurationManager.AppSettings["PathTicketsPending"];
            string filter = ConfigurationManager.AppSettings["Filter"];
            if (Directory.Exists(path_observer))
            {
                observer.Path = path_observer;
                observer.Filter = filter;
                observer.EnableRaisingEvents = true;

                this._ticketsrvcore = new TicketServiceCore(this._logger, this._proxyCore);

               // this._ticketsrvcore.update_file_cache(); // check files when service is offline

              //  this._ticketsrvcore.run();
                //  this._servicecore.CheckAvailableChacheFiles();

                this._logger.Info("Service", $"start monitoring - sprint {start_counter}", 100);

                return true;
            }
            else
            {
                if (string.IsNullOrEmpty(path_observer)) this._logger.Error("Service", $"Parameter PathTicketsPending is Empty or not exist, check the parameter PathTicketsPending on App.Config.", 100);
                if (!Directory.Exists(path_observer)) this._logger.Error("Service", $"Path {path_observer} not exist", 100);

                return false;
            }

            //return retval;
        }

        private bool StopMonitoring()
        {
            this._logger.Info("Service", $"stoping Task", 100);
            this._ticketsrvcore.stop();

            this._logger.Info("Service", $"stoping Observer", 100);

            this._logger.SuccessAudit("Service", $"observer.EnableRaisingEvents = {observer.EnableRaisingEvents}", 100);

            /*

            if (observer != null && observer.EnableRaisingEvents)
            {
                // this._repositoryCore = null;
                // this._proxyCore = null;

                observer.EnableRaisingEvents = false;

                this._proxyCli = null;
                this._filelog = null;
                this._logger = null;

                return true;

            }

            */

            return false;

        }



        /************************************************************************
                                            EVENTS
         
         ************************************************************************/

        private void backgroundWorker1_OnDoWork(object sender, DoWorkEventArgs e)
        {
            while (BkgProccessIsOn)
            {
                // principal Process here

                this._ticketsrvcore.ProccesPendingTickets();

                this._ticketsrvcore.create_response_file_list();

                this._ticketsrvcore.move_files_process();

                // report progress
                backgroundWorker1.ReportProgress(10);

            }
            this._logger.SuccessAudit("Service", $"backgroundWorker1.CancellationPending -> {backgroundWorker1.CancellationPending}", 100);
  
        }

        private void backgroundWorker1_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
          //  this._logger.SuccessAudit("Service", $"OnProgressChanged", 100);

        }

        private void backgroundWorker1_OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this._logger.SuccessAudit("Service", $"OnCompleted", 100);

        }



        private void observer_Created(object sender, FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            this._logger.Info("Service", $"file created, name: {file.Name} size: {file.Length} bytes.", 100);

            var status = utils.FileIsEmpty(e.FullPath) ? FileStatus.Empty : utils.IsFileReady(e.FullPath) ? FileStatus.Available : FileStatus.Busy;
            TicketFileDomain fileticket = new TicketFileDomain()
            {
                FileName = file.Name,
                FullPath = file.FullName,
                DateCreate = file.CreationTime,
                DateModified = file.LastWriteTime,
                Length = file.Length,
                Status = (int)status,
                DateNextAttempt = DateTime.Now
            };


            var result = _ticketsrvcore.TicketRepoCore.RegisterTicketFile(fileticket);

            //  var result =  this._repositoryCore.RegisterTicketFile(fileticket);

            if (result == CacheStatus.Conflict) filechanged(e.FullPath);

        }

        private void observer_Changed(object sender, FileSystemEventArgs e)
        {
            this.filechanged(e.FullPath);

        }



        private void observer_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void observer_Renamed(object sender, RenamedEventArgs e)
        {

        }



        public void filechanged(string path)
        {
            if (!utils.FileIsEmpty(path))
            {
                var file = new FileInfo(path);
                var domainfile = _ticketsrvcore.TicketRepoCore.FindTicketFile(file.Name);
                if (domainfile != null && (file.Length != domainfile.Length))
                {
                    // utils.IsFileReady(e.FullPath);

                    domainfile.Status = (int)(utils.IsFileReady(path) ? FileStatus.Available : FileStatus.Busy);
                    domainfile.Length = file.Length;
                    domainfile.FullPath = file.FullName;
                    domainfile.DateNextAttempt = DateTime.Now;

                    var result = _ticketsrvcore.TicketRepoCore.ModifyTicketFile(domainfile);

                    if (result == CacheStatus.Create) this._logger.Info("Service", $"file changed : {file.Name} size: {file.Length} bytes.", 100);

                }
            }

        }


        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }


    }
}
