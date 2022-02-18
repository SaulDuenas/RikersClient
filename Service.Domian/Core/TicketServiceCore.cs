using NetLogger.Implementation;
using Service.Domian.Core;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RikersProxy.ClientProxy;
using RikersProxy.Entities;
using static Service.Domian.Core.TicketRepositoryCore;

namespace Service.Domian.Core
{
    namespace tickets
    {
        public class TicketServiceCore
        {
            private byte FILE_RESPONSE_NO_CREATE = 0;
            private byte FILE_RESPONSE_CREATE = 1;

            private byte FILE_MOVE = 1;

            public string Extension { get; set; }
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
            private DateTime? toNextTime = null;

            public string NticketField = "NoTicket";

            private ProxyCore _proxyCore = null;
            private TicketRepositoryCore _ticketrepoCore = null;
            private AttemptTicketRepositoryCore _attemptticketrepoCore = null;
            private Logger _logger = null;

            public TicketServiceCore(Logger logger, ProxyCore proxycore)
            {
               
                Extension = "txt";

                this._logger = logger;
                this._proxyCore = proxycore;
                this.TicketRepoCore = new TicketRepositoryCore(this._logger);
                this._attemptticketrepoCore = new AttemptTicketRepositoryCore(this._logger);

                this.PendingPath = ConfigurationManager.AppSettings["PathTicketsPending"];
                this.DispatchedPath = ConfigurationManager.AppSettings["PathTicketsDispatched"];
                this.QuarantinePath = ConfigurationManager.AppSettings["PathTicketsQuarantine"];
                this.ResponsePath = ConfigurationManager.AppSettings["PathTicketsResponse"];

                this.SecondsWait = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SecondsWait"]) ? int.Parse(ConfigurationManager.AppSettings["SecondsWait"]):300;
                this.TotalAttemps = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["TotalAttemps"])? int.Parse(ConfigurationManager.AppSettings["TotalAttemps"]): 5;

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


            public void runWorker()
            {
                while (isRun)
                {
                    ProccesPendingTickets();

                    create_response_file_list();

                    move_files_process();
                }

                EndTask = true;

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
                var filelistfrompath = utils.GetFileList(this.PendingPath, Extension).ToList();
                var filecache = this.TicketRepoCore.GetTicketFileList();
                var file_diff = filecache == null? filelistfrompath.ToList() : filelistfrompath.Where(p => !(filecache.Select(s => s.FileName)).Contains(p.Name)).ToList();

                // add files to cache
                foreach (FileInfo file in file_diff)
                {
                    var status = utils.FileIsEmpty(file.FullName) ? StatusFile.Empty : utils.IsFileReady(file.FullName) ? StatusFile.Available : StatusFile.Busy;
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
                   if (result == Status.Create) this._logger.Info("update_file_cache", $"file created, name: {file.Name} size: {file.Length} bytes.", 100);
                };


                // change of files
                var estatus_lts02 = new List<int>() { (int)StatusFile.Available, (int)StatusFile.Busy, (int)StatusFile.Empty };
                var filecache2 = this.TicketRepoCore.GetTicketFiletoAttempt(estatus_lts02);
                var filelistfrompath2 = utils.GetFileList(this.PendingPath, Extension).ToList();

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

                        domainfile.Status = (int)(utils.IsFileReady(file.FullName) ? StatusFile.Available : StatusFile.Busy);
                        domainfile.Length = file.Length;
                        domainfile.FullPath = file.FullName;
                        domainfile.DateModified = file.LastWriteTime;
                        domainfile.DateNextAttempt = DateTime.Now;
                        var result = this.TicketRepoCore.ModifyTicketFile(domainfile);

                        if (result == Status.Create) this._logger.SuccessAudit("update_file_cache", $"file changed : {file.FileName} size: {file.Length} bytes.", 100);

                    }
                };

            }


            public void ProccesPendingTickets() 
            {
                try
                {
                    
                    var estatus_lts01 = new List<int>() { (int)StatusFile.Available, (int)StatusFile.TryAgain };
                    var filecache = TicketRepoCore.GetTicketFiletoAttempt(estatus_lts01);

                    foreach (TicketFileDomain ticketfile in filecache)
                    {
                        // 1. Validations
                        var file_ready = utils.IsFileReady(ticketfile.FullPath);
                        if (!file_ready) this._logger.Warning("ProccesPendingTickets", $"the file {ticketfile.FileName} not cannot be read,try in 5 minutes", 100);

                        var tokenOk = file_ready ? _proxyCore.TokenisAvailable() : false;
                        if (file_ready && !tokenOk)
                        {
                            this._logger.Warning("ProccesPendingTickets", $"token not available,try obtain in {SecondsWait} seconds", 100);
                            toNextTime = DateTime.Now.AddSeconds(SecondsWait);
                        }
                        else
                        {
                            toNextTime = null;
                        }

                        // validation ok ?
                        if (file_ready && tokenOk)
                        {
                            // Create Case Data 
                            var casedata = ParseCaseData(ticketfile.FullPath);
                            // Send casedata to Rikers API
                            var result = _proxyCore.CreateCase(casedata);
                            // write result on log 
                            result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100));
                            // update result on cache
                            update_create_case_cache(casedata.CustomerProblemNumber, ticketfile, result);
                        }
                        else
                        {
                            //ticketfile.NoTicket

                            ProxyResult result = new ProxyResult();

                            result.Code = System.Net.HttpStatusCode.OK;

                            update_create_case_cache(ticketfile.NoTicket, ticketfile, result);

                        }

                        
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

            private CaseData ParseCaseData(string filePath)
            {
                var source = utils.GetFileContentbyKeyPair(filePath);
                var NTicket = source[NticketField];

                // Create Case Data 
                CaseData retval = _proxyCore.getCaseData("Subject de creación de Caso", "REPORTING DEVICE", "MX", NTicket);

                return retval;
            }

            private void update_create_case_cache(string CustomerProblemNumber, TicketFileDomain ticketfile, ProxyResult result)
            {
                string TransactionId;
                DateTime TransactionDate;
                int attempts = ticketfile.Attempts + 1;

                TransactionId = result.CaseCreate != null ? result.CaseCreate.CommonArea != null ? result.CaseCreate.CommonArea.TransactionId : "NotAvailable" : "NotAvailable";
                TransactionDate = result.CaseCreate != null ? result.CaseCreate.CommonArea != null ? result.CaseCreate.CommonArea.TransactionDate.DateTime : DateTime.Now : DateTime.Now;

                StatusFile statusfile;
                string casenumber;
                DateTime? DateNextAttempt = null;
                string message="";

                if (result.Code.Equals(System.Net.HttpStatusCode.Created))
                {
                    statusfile = StatusFile.Dispached;
                    message = "N° de caso creado satisfactoriamente";
                    casenumber = result.CaseCreate != null ? result.CaseCreate.CaseNumber : "";
                }
                else
                {
                    statusfile = (attempts <= TotalAttemps ? StatusFile.TryAgain : StatusFile.Quarantine);
                    DateNextAttempt = DateTime.Now.AddSeconds(SecondsWait);
                    message = $"Petición no creada, intento {attempts} de {TotalAttemps}, proximo intento en {SecondsWait} segundos.";
                }

                ticketfile.Status = (int)statusfile;
                ticketfile.NoTicket = CustomerProblemNumber;
                ticketfile.Response = (int)result.Code;
                ticketfile.CaseNumber = result.CaseCreate != null ? result.CaseCreate.CaseNumber : "";
                ticketfile.TransactionId = TransactionId;
                ticketfile.TransactionDate = TransactionDate;
                ticketfile.Attempts = attempts;
                ticketfile.FileResponseCreated = FILE_RESPONSE_NO_CREATE;
                ticketfile.DateResponse = DateTime.Now;
                ticketfile.DateNextAttempt = DateNextAttempt;
                ticketfile.Message = message;

                // Mod ticket Status
                var resultrepo = TicketRepoCore.ModifyTicketFile(ticketfile);

                // Reg Attempt 
                result.Messages.ForEach(item => this._attemptticketrepoCore.RegisterTicketAttempt(new AttemptTicketDomain()
                {
                    NoTicket = CustomerProblemNumber,
                    NAttempt = attempts,
                    Response = (int)result.Code,
                    Type = item.Type,
                    Code = item.Code,
                    Message = item.Reason,
                    TransactionId = TransactionId,
                    TransactionDate = TransactionDate,
                    DateResponse = DateTime.Now
                }
                ));

                if (resultrepo == Status.Create) this._logger.SuccessAudit("update_create_case_cache", $"Cache updated of file: {ticketfile.FileName} - Attempt: { attempts }", 100);

            }



            public void create_response_file_list() 
            {
                try
                {
                    // ************** CREATE RESPONSE FILES *********************

                    var filecache_fr = TicketRepoCore.GetTicketFiletoProcessResponse();

                    foreach (TicketFileDomain ticketfile in filecache_fr)
                    {
                        string fullpath = string.Concat(this.ResponsePath, ticketfile.NoTicket, ".txt");

                        var result = CreateCaseResponseFile(fullpath,ticketfile);

                        ticketfile.FileResponseCreated = result ? FILE_RESPONSE_CREATE : FILE_RESPONSE_NO_CREATE;
                        ticketfile.FullPathResponse = result ? fullpath : "";
                        var resultrepo = TicketRepoCore.ModifyTicketFile(ticketfile);

                        var msgresult = resultrepo == Status.Create ? $"Cache updated of file: {ticketfile.FileName}" : $"Error of create fileresponse: {ticket.FileName}";
                        var typeLog = resultrepo == Status.Create ? EventLogEntryType.SuccessAudit : EventLogEntryType.Error;

                        this._logger.WriteLog("CreateCreateCaseResponseFile", typeLog, msgresult, 100);

                    }
                }

                catch (Exception ex)
                {

                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("create_response_file_list", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("create_response_file_list", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("create_response_file_list", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                }

            }

            public void move_files_process()
            {
                try
                {
                    var filecache = TicketRepoCore.GetTicketFiletoMoveList();
                    var filelistfrompath = utils.GetFileList(this.PendingPath, Extension).ToList();
                    var file_diff = filecache == null ? filelistfrompath.ToList() : filelistfrompath.Where(p => (filecache.Select(s => s.FileName)).Contains(p.Name)).ToList();

                    foreach (FileInfo file in file_diff)
                    {
                        var ticketFile = this.TicketRepoCore.FindTicketFile(file.Name);

                        var destiny_path = ticketFile.Status == (int)StatusFile.Dispached ? DispatchedPath : QuarantinePath;

                        var filepath_destiny = String.Concat(destiny_path, file.Name);

                        file.MoveTo(filepath_destiny);

                        // update cache
                        ticketFile.FileMove = FILE_MOVE;
                        ticketFile.FullPath = filepath_destiny;

                        var result = this.TicketRepoCore.ModifyTicketFile(ticketFile);

                        if (result == Status.Create) this._logger.Info("move_files_process", $"{file.Name} has moved to {destiny_path}", 100);

                    }
                }

                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("create_responce_runtime", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("create_responce_runtime", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("create_responce_runtime", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                }
            }

            public bool CreateCaseResponseFile(string fullpath,TicketFileDomain ticket)
            {
                
                List<string> lines = new List<string>() { string.Join("|", "NoTicket", ticket.NoTicket),
                                                          string.Join("|", "Respuesta",  ticket.Response),
                                                          string.Join("|", "CaseNumber", ticket.CaseNumber),
                                                          string.Join("|", "Message", ticket.Message),
                                                          string.Join("|", "DateResponse",  (ticket.DateResponse ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")),
                                                          string.Join("|", "TransactionId", ticket.TransactionId),
                                                          string.Join("|", "TransactionDate",  (ticket.TransactionDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"))
                                                    };

                return utils.createfile(fullpath, lines);

            }

        }

 
    }

}
