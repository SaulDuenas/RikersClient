using NetLogger.Implementation;
using Service.Domian.Core;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Service.Domian.Core.TicketRepositoryCore;

namespace Service.Domian.Core
{
    namespace tickets
    {
        public class TicketServiceCoreAsync
        {
            private byte FILE_RESPONSE_NO_CREATE = 0;
            private byte FILE_RESPONSE_CREATE = 1;
            public int TotalAttemps { get; set; }
            public int SecondsWait { get; set; }
            public string PendingPath { get; set; }
            public string QuarantinePath { get; set; }
            public string DispatchedPath { get; set; }
            public string ResponsePath { get; set; }
            public TicketRepositoryCore TicketRepoCore { get => _ticketrepoCore; set => _ticketrepoCore = value; }

            Thread ThRequestTickets = null;

            private bool isRun = false;
            private bool EndTask = false;


            public string NticketField = "NoTicket";

            private ProxyCoreAsync _proxyCore = null;
            private TicketRepositoryCore _ticketrepoCore = null;
            private AttemptTicketRepositoryCore _attemptticketrepoCore = null;
            private Logger _logger = null;

            public TicketServiceCoreAsync(Logger logger, ProxyCoreAsync proxycore)
            {

                TotalAttemps = 5;
                SecondsWait = 20;

                this._logger = logger;
                this._proxyCore = proxycore;
                this.TicketRepoCore = new TicketRepositoryCore(this._logger);
                this._attemptticketrepoCore = new AttemptTicketRepositoryCore(this._logger);

            }

            public void run()
            {
                isRun = true;
             //   Task.Run(() => CheckAvailableChacheFiles()).Wait();
   

                if (ThRequestTickets == null)
                {
                    ThRequestTickets = new Thread(new ThreadStart(runWorker));
                   // ThRequestTickets.Name = "TicketService";
                   // ThRequestTickets.IsBackground = true;
                }
                ThRequestTickets.Start();
            
            }

            public void stop()
            {

                isRun = false;

                while (!EndTask);

                if (ThRequestTickets != null)
                {
                    
                    ThRequestTickets.Abort();
                    ThRequestTickets = null;
                }

            }

            public void update_file_cache()
            {
                var filelistfrompath = utils.GetFileList(this.PendingPath, "txt").ToList();
                var filecache = this.TicketRepoCore.GetTicketFileList();
                var file_diff = filecache == null? filelistfrompath.ToList() : filelistfrompath.Where(p => !(filecache.Select(s => s.FileName)).Contains(p.Name)).ToList();

                // add files to cache
                foreach (FileInfo file in file_diff)
                {
                    var status = utils.FileIsEmpty(file.FullName) ? FileStatus.Empty : utils.IsFileReady(file.FullName) ? FileStatus.Available : FileStatus.Busy;
                    TicketFileDomain fileticket = new TicketFileDomain()
                    {
                        FileName = file.Name,
                        FullPath = file.FullName,
                        Length = file.Length,
                        Status = (int)status,
                        DateCreate = file.CreationTime,
                        DateModified = file.LastWriteTime,
                        // DateCreate = DateTime.Parse(file.CreationTime.ToString("yyyy-MM-dd")),
                        // DateModified = DateTime.Parse(file.LastWriteTime.ToString("yyyy-MM-dd")),
                        DateResponse = null,
                        DateNextAttempt = DateTime.Now
                    };

                    var result = this.TicketRepoCore.RegisterTicketFile(fileticket);
                   if (result == CacheStatus.Create) this._logger.Info("RepositoryCore", $"file created, name: {file.Name} size: {file.Length} bytes.", 100);
                };


                // change of files
                var estatus_lts02 = new List<int>() { (int)FileStatus.Available, (int)FileStatus.Busy, (int)FileStatus.Empty };
                var filecache2 = this.TicketRepoCore.GetTicketFiletoAttempt(estatus_lts02);
                var filelistfrompath2 = utils.GetFileList(this.PendingPath, "txt").ToList();

                var files = (from fl in filelistfrompath2
                             join fc in filecache2 on fl.Name equals fc.FileName
                             where (fl.Length != fc.Length)
                             select (fc.FileName, fl.Length, fl.LastWriteTime, fl.FullName)).ToList();


                foreach (var file in files)
                {
                    var domainfile = this.TicketRepoCore.FindTicketFile(file.FileName);
                    if (domainfile != null)
                    {
                        // utils.IsFileReady(e.FullPath);

                        domainfile.Status = (int)(utils.IsFileReady(file.FullName) ? FileStatus.Available : FileStatus.Busy);
                        domainfile.Length = file.Length;
                        domainfile.FullPath = file.FullName;
                        domainfile.DateModified = file.LastWriteTime;
                        domainfile.DateNextAttempt = DateTime.Now;
                        var result = this.TicketRepoCore.ModifyTicketFile(domainfile);

                        if (result == CacheStatus.Create) this._logger.SuccessAudit("RepositoryCore", $"file changed : {file.FileName} size: {file.Length} bytes.", 100);

                    }
                };

            }



            public  void runWorker()
            {
                while (isRun)
                {
                    ProccesPendingTickets();
                }

                EndTask = true;

            }


            public async void ProccesPendingTickets() 
            {
                try
                {
                    var estatus_lts01 = new List<int>() { (int)FileStatus.Available, (int)FileStatus.TryAgain };
                    var filecache = TicketRepoCore.GetTicketFiletoAttempt(estatus_lts01);

                    foreach (TicketFileDomain ticketfile in filecache)
                    {
                        var source = utils.GetFileContentbyKeyPair(ticketfile.FullPath);
                        var NTicket = source[NticketField];

                        string TransactionId;
                        DateTime TransactionDate;
                        int attempts = ticketfile.Attempts + 1;
                        if (await _proxyCore.TokenisOk())
                        {
                            ////   Create Case Data 
                            var casedata = _proxyCore.getCaseData("Subject de creación de Caso", "REPORTING DEVICE", "MX", NTicket);

                            var result = await _proxyCore.CreateCase(casedata);
                            // write log
                            //result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100));

                            if (result.Code.Equals(System.Net.HttpStatusCode.Created))
                            {
                                FileStatus statusfile = FileStatus.Dispached;
                                string casenumber = result.CaseCreate != null ? result.CaseCreate.CaseNumber : "";
                                TransactionId = result.CaseCreate != null ? result.CaseCreate.CommonArea != null ? result.CaseCreate.CommonArea.TransactionId : "NotAvailable" : "NotAvailable";
                                TransactionDate = result.CaseCreate != null ? result.CaseCreate.CommonArea != null ? result.CaseCreate.CommonArea.TransactionDate.DateTime : DateTime.Now : DateTime.Now;

                                ticketfile.Status = (int)statusfile;
                                ticketfile.NoTicket = NTicket;
                                ticketfile.Response = (int)result.Code;
                                ticketfile.CaseNumber = casenumber;
                                ticketfile.TransactionId = TransactionId;
                                ticketfile.TransactionDate = TransactionDate;
                                ticketfile.Attempts = attempts;
                                ticketfile.FileResponseCreated = FILE_RESPONSE_NO_CREATE;
                                ticketfile.DateResponse = DateTime.Now;

                            }
                            else
                            {
                                TransactionId = result.ResponseMessage != null ? result.ResponseMessage.TransactionId : "NotAvailable";
                                TransactionDate = result.ResponseMessage != null ? result.ResponseMessage.TransactionDate.DateTime  : DateTime.Now;
                                
                                // se registra n de intento de la creación del caso
                                ticketfile.Status = (int)(ticketfile.Attempts <= TotalAttemps ? FileStatus.TryAgain : FileStatus.Quarantine);
                                ticketfile.Response = (int)result.Code;
                                ticketfile.Attempts = attempts;
                                ticketfile.NoTicket = NTicket;
                                ticketfile.DateResponse = DateTime.Now;
                                ticketfile.DateNextAttempt = DateTime.Now.AddSeconds(SecondsWait);

                            }

                            // Mod ticket Status
                            var resultrepo = TicketRepoCore.ModifyTicketFile(ticketfile);

                            // Reg Attempt 
                            result.Messages.ToList().ForEach(r => this._attemptticketrepoCore.RegisterTicketAttempt(
                            new AttemptTicketDomain()
                            {
                                NoTicket = NTicket,
                                NAttempt = attempts,
                                Response = (int)result.Code,
                                Type = r.Type,
                                Code = r.Code,
                                Message = r.Reason,
                                TransactionId = TransactionId,
                                TransactionDate = TransactionDate,
                                DateResponse = DateTime.Now
                            }
                            ));

                            if (resultrepo == CacheStatus.Create) this._logger.SuccessAudit("ProccesPendingTickets", $"Cache updated of file: {ticketfile.FileName}", 100);


                        }

                    }

                    var estatusfilelts = new List<int>() { (int)FileStatus.Dispached, (int)FileStatus.Quarantine };
                    var filecache_fr = TicketRepoCore.GetTicketFiletoAttempt(estatusfilelts);
                    filecache_fr = filecache_fr.Where(p => p.FileResponseCreated == FILE_RESPONSE_NO_CREATE).ToList();

                    foreach (TicketFileDomain ticketfile in filecache_fr)
                    {
                        CreateCreateCaseResponseFile(ticketfile);

                    }

                }
                catch (Exception ex)
                {

                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("ProccesPendingTickets", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("ProccesPendingTickets", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("ProccesPendingTickets", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                }

            }




            private void regAttempt( int attempt, int code,string type,string scode, string message,string transactionid,DateTime transactiondate,DateTime dateresponse ) {

                var destination = new AttemptTicketDomain()
                {
                    NAttempt = attempt,
                    Response = code,
                    Type = type, 
                };

                this._attemptticketrepoCore.RegisterTicketAttempt(destination);

            }



            public void CreateCreateCaseResponseFile(TicketFileDomain ticket)
            {
                string fullpath = string.Concat(this.ResponsePath, ticket.NoTicket, ".txt");

               

                var message = ticket.Status.Equals((int)FileStatus.Dispached) ? "N° de caso creado satisfactoriamente" :
                              ticket.Status.Equals((int)FileStatus.Quarantine) ? "Problema al crear el N. de caso" : "";

                List<string> lines = new List<string>() { string.Join("|", "NoTicket", ticket.NoTicket),
                                                          string.Join("|", "Respuesta",  ticket.Response),
                                                          string.Join("|", "CaseNumber", ticket.CaseNumber),
                                                          string.Join("|", "Message", message),
                                                          string.Join("|", "DateResponse",  (ticket.DateResponse ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")),
                                                          string.Join("|", "TransactionId", ticket.TransactionId),
                                                          string.Join("|", "TransactionDate",  (ticket.TransactionDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"))
                                                    };

                var result = utils.createfile(fullpath, lines);

                ticket.FileResponseCreated = result ? FILE_RESPONSE_CREATE : FILE_RESPONSE_NO_CREATE;
                ticket.FullPathResponse = result ? fullpath : "";
                var resultrepo = TicketRepoCore.ModifyTicketFile(ticket);

                var msgresult = resultrepo == CacheStatus.Create ? $"Cache updated of file: {ticket.FileName}" : $"Error of create fileresponse: {ticket.FileName}";
                var typeLog = resultrepo == CacheStatus.Create ? EventLogEntryType.SuccessAudit : EventLogEntryType.Error;

                this._logger.WriteLog("RepositoryCore", typeLog, msgresult, 100);
            }


        }

 
    }

}
