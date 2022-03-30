
namespace ServiceApp
{
    partial class TicketService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.obsPathtickets = new System.IO.FileSystemWatcher();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.obsPathFeedBack = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.obsPathtickets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.obsPathFeedBack)).BeginInit();
            // 
            // obsPathtickets
            // 
            this.obsPathtickets.EnableRaisingEvents = true;
            this.obsPathtickets.Changed += new System.IO.FileSystemEventHandler(this.obsPathtickets_Changed);
            this.obsPathtickets.Created += new System.IO.FileSystemEventHandler(this.obsPathtickets_Created);
            this.obsPathtickets.Deleted += new System.IO.FileSystemEventHandler(this.obsPathtickets_Deleted);
            this.obsPathtickets.Renamed += new System.IO.RenamedEventHandler(this.obsPathtickets_Renamed);
            // 
            // eventLog1
            // 
            this.eventLog1.EntryWritten += new System.Diagnostics.EntryWrittenEventHandler(this.eventLog1_EntryWritten);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            // 
            // obsPathFeedBack
            // 
            this.obsPathFeedBack.EnableRaisingEvents = true;
            this.obsPathFeedBack.Changed += new System.IO.FileSystemEventHandler(this.obsPathFeedBack_Changed);
            this.obsPathFeedBack.Created += new System.IO.FileSystemEventHandler(this.obsPathFeedBack_Created);
            this.obsPathFeedBack.Deleted += new System.IO.FileSystemEventHandler(this.obsPathFeedBack_Deleted);
            this.obsPathFeedBack.Renamed += new System.IO.RenamedEventHandler(this.obsPathFeedBack_Renamed);
            // 
            // TicketService
            // 
            this.CanShutdown = true;
            this.ServiceName = "TicketService";
            ((System.ComponentModel.ISupportInitialize)(this.obsPathtickets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.obsPathFeedBack)).EndInit();

        }

        #endregion

        private System.IO.FileSystemWatcher obsPathtickets;
        private System.Diagnostics.EventLog eventLog1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.IO.FileSystemWatcher obsPathFeedBack;
    }
}
