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


namespace consoleApp
{
    public partial class Form1 : Form
    {
        private int start_counter = 0;

        private string _exePath;
        
        private ILogger _logger = null;

        private Winlogger _locallog = null;
        private FileLogger _filelog = null;

       // private ProxyCore _proxyCore = null;
        private TicketServiceCore _ticketsrvcore = null;
        private FeedBackServiceCore _feedbacksrvcore = null;

        public Form1()
        {
            InitializeComponent();
            InitializeWindowForm();

          //  this._proxyCli = clientProxy;
            this._locallog = new Winlogger(); //  new Locallogger(listBox1);
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
          //  this._proxyCli = new ClientProxy();
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
            
            if (StopTicketsMonitoring() && StopFeedBackMonitoring())
            {
                btnstart.Enabled = true;
                btnStop.Enabled = false;
                button1.Enabled = false;
            }


            this._filelog = null;
            this._logger = null;

        }

        private bool startMonitoring() 
        {
            bool retval= false;

            _locallog.ListBoxRef = listBox1;

            this._logger.LogAppender.Add(_locallog);
            this._logger.LogAppender.Add(_filelog);

            start_counter++;

            this._ticketsrvcore = new TicketServiceCore(this._logger);
            retval = this._ticketsrvcore.run();   // ticketService

            if (retval)
            {
                obsTickets.Path = ConfigurationManager.AppSettings["PathTicketsPending"]; 
                obsTickets.Filter = ConfigurationManager.AppSettings["Filter"];
                obsTickets.EnableRaisingEvents = true;
            }

            this._feedbacksrvcore = new FeedBackServiceCore(this._logger);
            retval=this._feedbacksrvcore.run();   // FeedBackService

            if (retval)
            {
                obsFeedBack.Path = ConfigurationManager.AppSettings["PathCommentsPending"]; ;
                obsFeedBack.Filter = ConfigurationManager.AppSettings["Filter"];
                obsFeedBack.EnableRaisingEvents = true;
            }

            if(retval) this._logger.Info("UITesting", $"star monitoring - sprint {start_counter}", 100);

            return retval;
        }

        private bool StopTicketsMonitoring() 
        {
            this._ticketsrvcore.stop();
            
            if (obsTickets.EnableRaisingEvents)
            {

                obsTickets.EnableRaisingEvents = false;

                this._logger.Info("UITesting", $"stop tickets monitoring - sprint {start_counter}", 100);

                return true;
            }

            return false;
            
        }

        private bool StopFeedBackMonitoring()
        {
            this._feedbacksrvcore.stop();

            if (obsFeedBack.EnableRaisingEvents)
            {

                obsFeedBack.EnableRaisingEvents = false;

                this._logger.Info("UITesting", $"stop feedback monitoring - sprint {start_counter}", 100);

                return true;
            }

            return false;

        }

        private async void  button1_Click(object sender, EventArgs e)
        {

            button1.Enabled = false;

            this._logger.Info("UITesting", $"click en boton ", 100);

            button1.Enabled = true;
            //generateToken();
        }





        /*
         
              Eventos de identificacion de archivos de tickets
         
         */

        private void obsTickets_Created(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("UITesting", $"Ticket file created, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._ticketsrvcore.RegisterFileTickettoCache(e.FullPath);

        }

        private void obsTickets_Changed(object sender, FileSystemEventArgs e)
        {
            var result = this._ticketsrvcore.UpdateFileTicketCache(e.FullPath);

            if (result) this._logger.Info("UITesting", $"Ticket file changed : {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

        }

        private void obsTickets_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void obsTickets_Renamed(object sender, RenamedEventArgs e)
        {

        }

        /*

            Eventos de identificacion de archivos de comentarios

       */

        private void obsComments_Changed(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("UITesting", $"Comment file changed, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._feedbacksrvcore.UpdateFileCommentCache(e.FullPath);
        }

        private void obsComments_Created(object sender, FileSystemEventArgs e)
        {
            this._logger.Info("UITesting", $"Comment file created, name: {e.Name} size: {new FileInfo(e.FullPath).Length} bytes.", 100);

            this._feedbacksrvcore.RegisterFileFeedBacktoCache(e.FullPath);
        }


        private void obsComments_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void obsComments_Renamed(object sender, RenamedEventArgs e)
        {

        }


        private void ListBox_DoubleClick(object sender, EventArgs e) 
        {
            Clipboard.SetText(listBox1.SelectedItem.ToString());
        }


    }


   
}
