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
        private bool StopBkgProcess = false;

        private int start_counter = 0;

        private string _exePath;
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

        public void OnDebug() 
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart.");

            // TODO: Add code here to start your service.

            this._proxyCli = new ClientProxy();
            this._filelog = new FileLogger();
            this._logger = new Logger();

            if (startMonitoring()) {

                if (this.backgroundWorker1.IsBusy != true)
                {
                    BkgProccessIsOn = true;
                    backgroundWorker1.RunWorkerAsync();
                }
            }

            eventLog1.WriteEntry("End OnStart.");
        }

        protected override void OnStop()
        {

            eventLog1.WriteEntry("In OnStop.");
            backgroundWorker1.CancelAsync();

            // TODO: Add code here to perform any tear-down necessary to stop your service.

            if (StopMonitoring())
            {
                this._proxyCli = null;
                this._filelog = null;
                this._logger = null;

            }

            eventLog1.WriteEntry("End OnStop.");
        }

        protected override void OnShutdown()
        {
            eventLog1.WriteEntry("In OnShutdown.");
        }



        private bool startMonitoring()
        {

            bool retval = false;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["Logger"]) )
            {
                _filelog.strEventLogSelect = ConfigurationManager.AppSettings["Logger"];
                this._logger.LogAppender.Add(_filelog);
            }
            start_counter++;

            if (!(String.IsNullOrEmpty(ConfigurationManager.AppSettings["BaseUrl"]) && String.IsNullOrEmpty(ConfigurationManager.AppSettings["credential"])))
            {
                retval = true;

                this._proxyCli.BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                this._proxyCli.Credentials = ConfigurationManager.AppSettings["credential"];
                this._proxyCli.EndPointCreateCase = ConfigurationManager.AppSettings["CreateCase"];
                this._proxyCli.EndPointGetToken = ConfigurationManager.AppSettings["EndPointAccessToken"];
               // this._proxyCli.SerialCert = ConfigurationManager.AppSettings["SerialCert"];

                this._proxyCore = new ProxyCore(this._proxyCli, this._logger);

            }
            else
            {

                this._logger.Error("Service", $" BaseUrl and/or credential initialize parameters not found", 100);

                return false;
            }


            if (retval && Directory.Exists(ConfigurationManager.AppSettings["TicketsPending"]))
            {
                observer.Path = ConfigurationManager.AppSettings["TicketsPending"];
                observer.Filter = ConfigurationManager.AppSettings["Filter"];
                observer.EnableRaisingEvents = true;

                this._ticketsrvcore = new TicketServiceCore(this._logger, this._proxyCore);

                this._ticketsrvcore.PendingPath = ConfigurationManager.AppSettings["TicketsPending"];
                this._ticketsrvcore.DispatchedPath = ConfigurationManager.AppSettings["TicketsDispatched"];
                this._ticketsrvcore.QuarantinePath = ConfigurationManager.AppSettings["TicketsQuarantine"];
                this._ticketsrvcore.ResponsePath = ConfigurationManager.AppSettings["TicketsResponse"];

                this._ticketsrvcore.TotalAttemps = int.Parse(ConfigurationManager.AppSettings["TotalAttemps"]);
                this._ticketsrvcore.SecondsWait = int.Parse(ConfigurationManager.AppSettings["SecondsWait"]);

                this._ticketsrvcore.update_file_cache(); // check files when service is offline

              //  this._ticketsrvcore.run();
                //  this._servicecore.CheckAvailableChacheFiles();

                this._logger.Info("Service", $"start monitoring - sprint {start_counter}", 100);

                return true;
            }
            else
            {
                this._logger.Error("Service", $"Directory name  {ConfigurationManager.AppSettings["TicketsPending"]} is not valid.", 100);

                return false;
            }

            //return retval;
        }

        private bool StopMonitoring()
        {
            BkgProccessIsOn = false;

            while (!StopBkgProcess);

            // this._ticketsrvcore.stop();


            if (observer.EnableRaisingEvents)
            {

                // this._repositoryCore = null;
                // this._proxyCore = null;

                observer.EnableRaisingEvents = false;

                this._logger.Info("Service", $"stop monitoring - sprint {start_counter}", 100);

                return true;
            }

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
            StopBkgProcess = true;

            if (backgroundWorker1.CancellationPending == true)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker1_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }



        private void observer_Created(object sender, FileSystemEventArgs e)
        {
            var file = new FileInfo(e.FullPath);
            this._logger.Info("Service", $"file created, name: {file.Name} size: {file.Length} bytes.", 100);

            var status = utils.FileIsEmpty(e.FullPath) ? StatusFile.Empty : utils.IsFileReady(e.FullPath) ? StatusFile.Available : StatusFile.Busy;
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

            if (result == Status.Conflict) filechanged(e.FullPath);

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

                    domainfile.Status = (int)(utils.IsFileReady(path) ? StatusFile.Available : StatusFile.Busy);
                    domainfile.Length = file.Length;
                    domainfile.FullPath = file.FullName;
                    domainfile.DateNextAttempt = DateTime.Now;

                    var result = _ticketsrvcore.TicketRepoCore.ModifyTicketFile(domainfile);

                    if (result == Status.Create) this._logger.Info("Service", $"file changed : {file.Name} size: {file.Length} bytes.", 100);

                }
            }

        }


        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }


    }
}
