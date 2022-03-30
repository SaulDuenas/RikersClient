
namespace consoleApp
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStop = new System.Windows.Forms.Button();
            this.btnstart = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtProblemNumber = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.txtfilter = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtInboundPath = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.obsTickets = new System.IO.FileSystemWatcher();
            this.obsFeedBack = new System.IO.FileSystemWatcher();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.obsTickets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.obsFeedBack)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(493, 47);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(106, 28);
            this.btnStop.TabIndex = 13;
            this.btnStop.Text = "Detener Monitoreo";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnstart
            // 
            this.btnstart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnstart.Location = new System.Drawing.Point(494, 13);
            this.btnstart.Name = "btnstart";
            this.btnstart.Size = new System.Drawing.Size(106, 28);
            this.btnstart.TabIndex = 11;
            this.btnstart.Text = "Iniciar Monitoreo";
            this.btnstart.UseVisualStyleBackColor = true;
            this.btnstart.Click += new System.EventHandler(this.btnstart_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtProblemNumber);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnStop);
            this.groupBox1.Controls.Add(this.txtUrl);
            this.groupBox1.Controls.Add(this.btnstart);
            this.groupBox1.Controls.Add(this.txtfilter);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtInboundPath);
            this.groupBox1.Location = new System.Drawing.Point(7, -1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(609, 165);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Problem number:";
            // 
            // txtProblemNumber
            // 
            this.txtProblemNumber.Location = new System.Drawing.Point(105, 106);
            this.txtProblemNumber.Name = "txtProblemNumber";
            this.txtProblemNumber.Size = new System.Drawing.Size(94, 20);
            this.txtProblemNumber.TabIndex = 27;
            this.txtProblemNumber.Text = "ws60001";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(495, 92);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 23);
            this.button1.TabIndex = 26;
            this.button1.Text = "Crear Ticket Att";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "url Servicio:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(105, 75);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(309, 20);
            this.txtUrl.TabIndex = 23;
            // 
            // txtfilter
            // 
            this.txtfilter.Enabled = false;
            this.txtfilter.Location = new System.Drawing.Point(105, 40);
            this.txtfilter.Name = "txtfilter";
            this.txtfilter.Size = new System.Drawing.Size(103, 20);
            this.txtfilter.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Filtrar por:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Ruta a Monitorear:";
            // 
            // txtInboundPath
            // 
            this.txtInboundPath.Location = new System.Drawing.Point(105, 14);
            this.txtInboundPath.Name = "txtInboundPath";
            this.txtInboundPath.Size = new System.Drawing.Size(309, 20);
            this.txtInboundPath.TabIndex = 19;
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(6, 170);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(610, 212);
            this.listBox1.TabIndex = 20;
            // 
            // obsTickets
            // 
            this.obsTickets.EnableRaisingEvents = true;
            this.obsTickets.SynchronizingObject = this;
            this.obsTickets.Changed += new System.IO.FileSystemEventHandler(this.obsTickets_Changed);
            this.obsTickets.Created += new System.IO.FileSystemEventHandler(this.obsTickets_Created);
            this.obsTickets.Deleted += new System.IO.FileSystemEventHandler(this.obsTickets_Deleted);
            this.obsTickets.Renamed += new System.IO.RenamedEventHandler(this.obsTickets_Renamed);
            // 
            // obsComments
            // 
            this.obsFeedBack.EnableRaisingEvents = true;
            this.obsFeedBack.SynchronizingObject = this;
            this.obsFeedBack.Changed += new System.IO.FileSystemEventHandler(this.obsComments_Changed);
            this.obsFeedBack.Created += new System.IO.FileSystemEventHandler(this.obsComments_Created);
            this.obsFeedBack.Deleted += new System.IO.FileSystemEventHandler(this.obsComments_Deleted);
            this.obsFeedBack.Renamed += new System.IO.RenamedEventHandler(this.obsComments_Renamed);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 389);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.obsTickets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.obsFeedBack)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnstart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.TextBox txtfilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInboundPath;
        private System.Windows.Forms.ListBox listBox1;
        private System.IO.FileSystemWatcher obsTickets;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtProblemNumber;
        private System.IO.FileSystemWatcher obsFeedBack;
    }
}

