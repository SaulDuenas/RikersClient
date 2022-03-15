using Monitor.Directory;
using NetLogger.Implementation;
using RikersProxy;
using RikersProxy.Entities;
using RikersProxy.Entities.General;
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Service.Domian.Core.TicketRepositoryCore;

namespace consoleApp
{
    public partial class Form1 : Form
    {
        private int start_counter = 0;

        private string _exePath;
      //  private EventViewertExt _eventExt = null;
        private IClientProxy _proxyCli = null;
        
        private Logger _logger = null;

        private Winlogger _locallog = null;
        private FileLogger _filelog = null;

        private ProxyCore _proxyCore = null;
        private TicketServiceCore _ticketsrvcore = null;

        public Form1(IClientProxy clientProxy, Winlogger locallog, FileLogger filelog, Logger logger)
        {
            InitializeComponent();
            InitializeWindowForm();

          //  this._proxyCli = clientProxy;
            this._locallog = locallog; //  new Locallogger(listBox1);
         //   this._filelog = filelog;
         //   this._logger = logger;


            this.Text = $"Consola de Pruebas v{Assembly.GetExecutingAssembly().GetName().Version.ToString()}";

            _exePath = AppDomain.CurrentDomain.BaseDirectory;

        }


        private void InitializeWindowForm()
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            btnstart.Enabled = true;
            btnStop.Enabled = false;
            button1.Enabled = false;

            txtUrl.Text = ConfigurationManager.AppSettings["BaseUrl"];
            txtInboundPath.Text = ConfigurationManager.AppSettings["PathTicketsPending"];
            txtfilter.Text = ConfigurationManager.AppSettings["Filter"];
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            this._proxyCli = new ClientProxy();
            this._filelog = new FileLogger();
            this._logger = new Logger();

            if (startMonitoring())
            {
                btnstart.Enabled = false;
                btnStop.Enabled = true;
               // button1.Enabled = true;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
         
            if (StopMonitoring())
            {
                btnstart.Enabled = true;
                btnStop.Enabled = false;
                button1.Enabled = false;
            }

            this._proxyCli = null;
            this._filelog = null;
            this._logger = null;

        }

        private bool startMonitoring() 
        {
            _locallog.ListBoxRef = listBox1;

            this._logger.LogAppender.Add(_locallog);
            this._logger.LogAppender.Add(_filelog);

            start_counter++;

            this._proxyCore = new ProxyCore(this._proxyCli, this._logger);

            /*


            if (!(String.IsNullOrEmpty(ConfigurationManager.AppSettings["BaseUrl"]) && String.IsNullOrEmpty(ConfigurationManager.AppSettings["credential"])))
            {
                retval = true;

               // this._proxyCli.BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
               // this._proxyCli.Credentials = ConfigurationManager.AppSettings["credential"];
               // this._proxyCli.EndPointCreateCase = ConfigurationManager.AppSettings["CreateCase"];
              //  this._proxyCli.EndPointGetToken = ConfigurationManager.AppSettings["EndPointAccessToken"];

                this._proxyCore = new ProxyCore(this._proxyCli, this._logger);

               // this._client.SerialCert = ConfigurationManager.AppSettings["SerialCert"];
            }
            else {

                this._logger.Error("UITesting", $" BaseUrl and/or credential initialize parameters not found", 100);

                return false;
            }

            */

            string path_observer = ConfigurationManager.AppSettings["PathTicketsPending"];

            if (!string.IsNullOrEmpty(path_observer) && Directory.Exists(path_observer))
            {
                observer.Path = path_observer;
                observer.Filter = ConfigurationManager.AppSettings["Filter"];
                observer.EnableRaisingEvents = true;

                this._ticketsrvcore = new TicketServiceCore(this._logger, this._proxyCore);

                this._ticketsrvcore.update_file_cache();

                this._ticketsrvcore.run();
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
            this._ticketsrvcore.stop();
            if (observer.EnableRaisingEvents) {

               // this._repositoryCore = null;
               // this._proxyCore = null;

                observer.EnableRaisingEvents = false;

                this._logger.Info("Service", $"stop monitoring - sprint {start_counter}", 100);

                return true;
            }

            return false;
            
        }

        private async void  button1_Click(object sender, EventArgs e)
        {

            button1.Enabled = false;

            if (TokenisOk())
            {
                var casedata = getCaseData("Subject de creación de Caso", "REPORTING DEVICE", "MX", txtProblemNumber.Text);

                var result = _proxyCli.CreateCase( casedata);
                // write log
                result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100) );
             
            }

            button1.Enabled = true;
            //generateToken();
        }





        /*
         
              Eventos de identificacion de archivos
         
         
         */

        private void observer_Created(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("UITesting", $"file created, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._ticketsrvcore.RegisterFileTickettoCache(e.FullPath);

        }

        private void observer_Changed(object sender, FileSystemEventArgs e)
        {
            var result = this._ticketsrvcore.UpdateFileTicketCache(e.FullPath);

            if (result) this._logger.Info("UITesting", $"file changed : {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);


        }



        private void observer_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void observer_Renamed(object sender, RenamedEventArgs e)
        {

        }



        public bool TokenisOk() 
        {

            bool tokenOk = true;

            if (!(_proxyCli.Elapsed_Time_Token.Equals(null)) && _proxyCli.Elapsed_Time_Token.TotalSeconds <= 0)
            {
                this._logger.Info("UITesting","token expired, requesting ner token",100);

                var result = _proxyCli.TokenRequest();
                
                if (result.Code.Equals(HttpStatusCode.OK))
                {
                    result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100));
                   
                    this._logger.WriteLog("CREATE TOKEN","SuccessAudit", $"Request Code: {(int)result.Code} - {result.Message} - AccessToken: {_proxyCli.Token.AccessToken} - ExpiresIn: {_proxyCli.Token.ExpiresIn}", 100);
                 
                    tokenOk = true;
                }
                else
                {
                    result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100));
                  
                    tokenOk = false;
                  //  _logger.Error("UITesting", $"Request Code: {(int)result.Code} - Message: {result.Message}", 100);
                }

            }

            return tokenOk;

        }

        static public CaseData getCaseData(string subject, string description,string country,string customerproblemnumber  )
        {

            var casedata = new CaseData();

            casedata.Subject = subject;
            casedata.Description = description;
            casedata.Country = country;
            casedata.CustomerProblemNumber = customerproblemnumber;

            casedata.Customer = new Customer(){ CompanyName = ConfigurationManager.AppSettings["CompanyName"], 
                                                IbmCustomerNumber = ConfigurationManager.AppSettings["IbmCustomerNumber"]
            };

            casedata.CaseContact = new CaseContact() { GivenName="Donald", FamilyName="Duck", Phone="+555555", Email="donald@duck.false" };

            casedata.Asset = new Asset() { IbmMachineType="MF32", IbmMachineModel="000", Serial="MX46709" };

            return casedata;
        }

        private void ListBox_DoubleClick(object sender, EventArgs e) 
        {
            Clipboard.SetText(listBox1.SelectedItem.ToString());
        }

    }


   
}
