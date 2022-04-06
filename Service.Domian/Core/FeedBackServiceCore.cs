using NetLogger.Implementation;
using RikersProxy;
using RikersProxy.Entities;
using Service.Domian.Core.Proxy;
using Service.Domian.Core.Repo;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RikersProxy.ClientProxy;

namespace Service.Domian.Core
{
    public class FeedBackServiceCore
    {
     
        public string Extension { get; set; }
        public int TotalAttemps { get; set; }
        public int SecondsWait { get; set; }
        public string PendingPath { get; set; }
        public string QuarantinePath { get; set; }
        public string DispatchedPath { get; set; }
        public string ResponsePath { get; set; }

        Thread thRequestComments = null;

        private bool isRun = false;
        private bool EndTask = false;

        public string NTICKETFIELD = "NoTicket";
        public string CASENUMBERFIELD = "CaseNumber";
        public string BODYFIELD = "Body";

        private ProxyCore _proxyCore = null;
        private FeedBackRepositoryCore _FeedBackRepoCore = null;
        private AttemptTicketRepositoryCore _attemptticketrepoCore = null;
        private ILogger _logger = null;

        public FeedBackServiceCore(ILogger logger)
        {
            this.isRun = false;
            this.EndTask = true;
            Extension = "txt";

            this._logger = logger;

            IClientProxy proxyCli = ClientProxy.Instance;
            this._proxyCore = new ProxyCore(proxyCli, this._logger);

            this._FeedBackRepoCore = new FeedBackRepositoryCore(this._logger);
            this._attemptticketrepoCore = new AttemptTicketRepositoryCore(this._logger);

            this.PendingPath = ConfigurationManager.AppSettings["PathCommentsPending"];
            this.DispatchedPath = ConfigurationManager.AppSettings["PathCommentsDispatched"];
            this.QuarantinePath = ConfigurationManager.AppSettings["PathCommentsQuarantine"];
            this.ResponsePath = ConfigurationManager.AppSettings["PathCommentsResponse"];

            this.SecondsWait = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SecondsWait"]) ? int.Parse(ConfigurationManager.AppSettings["SecondsWait"]) : 300;
            this.TotalAttemps = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["TotalAttemps"]) ? int.Parse(ConfigurationManager.AppSettings["TotalAttemps"]) : 5;

        }

        private bool CheckParameters()
        {
            bool retval = true;

            if (!String.IsNullOrEmpty(this.PendingPath) && !Directory.Exists(this.PendingPath))
            {
                this._logger.Error("FeedBackServiceCore", $"Parameter PathCommentsPending is Empty or not exist, check the parameter PathCommentsPending on App.Config.", 100);
                return false;
            }
            if (!String.IsNullOrEmpty(this.DispatchedPath) && !Directory.Exists(this.DispatchedPath))
            {
                this._logger.Error("FeedBackServiceCore", $"Parameter PathCommentsDispatched is Empty or not exist, check the parameter PathCommentsDispatched on App.Config.", 100);
                return false;
            }
            if (!String.IsNullOrEmpty(this.QuarantinePath) && !Directory.Exists(this.QuarantinePath))
            {
                this._logger.Error("FeedBackServiceCore", $"Parameter PathCommentsQuarantine is Empty or not exist, check the parameter PathCommentsQuarantine on App.Config.", 100);
                return false;
            }
            if (!String.IsNullOrEmpty(this.ResponsePath) && !Directory.Exists(this.ResponsePath))
            {
                this._logger.Error("FeedBackServiceCore", $"Parameter PathCommentsResponse is Empty or not exist, check the parameter PathCommentsResponse on App.Config.", 100);
                return false;
            }

            return retval;

        }

        public bool run()
        {
            bool retval = false;
            this.isRun = true;

            retval = CheckParameters();

            try
            {

                if (retval && thRequestComments == null)
                {
                    ThreadStart start = new ThreadStart(runWorker);
                    thRequestComments = new Thread(start);
                    thRequestComments.Name = "CommentService";

                    thRequestComments.Start();
                }
                
            }
            catch (Exception)
            {
                throw;
            }

            return retval;

        }


        private void runWorker()
        {

            Thread.Sleep(500);
            this.EndTask = false;
            this._logger.SuccessAudit("FeedBackServiceCore", $"Starting thread, update cache  when service is offline", 100);

            this.AttachFilesCache(); // check files when service is offline  NOTE. por definir

            this._logger.SuccessAudit("FeedBackServiceCore", $"Start Cycle", 100);

            while (this.isRun)
            {
                ProccesPendingFeedBacks();

                CreateResponseFileList(); 

                MoveFilesProcess();       // SADB por definir
            }

            this.EndTask = true;

        }

        public void stop()
        {

            try
            {

                this.isRun = false;

                while (!this.EndTask)
                {
                    this._logger.SuccessAudit("TicketServiceCore", $"Stoping Thread, waiting to end the last cycle", 100);
                    Thread.Sleep(500);
                }

                this._logger.SuccessAudit("TicketServiceCore", $"finish the last cycle", 100);

                if ((thRequestComments != null) & thRequestComments.IsAlive)
                {
                    thRequestComments.Abort();
                    thRequestComments = null;

                    this._logger.SuccessAudit("TicketServiceCore", $"stop thread finish", 100);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void ProccesPendingFeedBacks()
        {

            try
            {

                var filecache = _FeedBackRepoCore.GetFeedBackPendingProcess();

                foreach (FeedBackFileDomain commentfile in filecache)
                {
                    // 1. Validations
                    var file_ready = utils.IsFileReady(commentfile.FullPath);
                    if (!file_ready) this._logger.Warning("ProccesPendingFeedBacks", $"the file {commentfile.FileName} not cannot be read,try in 5 minutes", 100);

                    var tokenOk = file_ready ? _proxyCore.TokenisAvailable() : false;

                    // validation ok ?
                    if (file_ready && tokenOk)
                    {
                        var commentdata = ParseFeedBackData(commentfile.FullPath);  // Create Case Data 
                        var result = _proxyCore.SubmitFeedback(commentdata); // Send commentdata to Rikers API
                        result.Messages.ToList().ForEach(item => this._logger.WriteLog(item.Category, item.Type, $"Request Code: {item.Code} - Message: {item.Reason}", 100));  // write result on log 

                        UpdateFileCommentCache(commentdata.ExternalProblemNumber,commentdata.CaseNumber, commentfile, result); // update result on cache

                    }

                    else if (!file_ready || !tokenOk)
                    {
                        string message = "";
                        HttpStatusCode code = System.Net.HttpStatusCode.Conflict;
                        message = !file_ready ? $"file not available,try obtain in {SecondsWait} seconds" :
                                  (file_ready && !tokenOk) ? $"token not available,try obtain in { SecondsWait} seconds" : "";

                        this._logger.Warning("ProccesPendingTickets", message, 100);  // notify 

                        ProxyResult result = new ProxyResult() { Code = System.Net.HttpStatusCode.Conflict };
                        // var casedata = ParseCaseData(ticketfile.FullPath);   // getdata 
                        result.Messages = new List<RikersProxy.ClientProxy.Message>();
                        result.Messages.Add(new RikersProxy.ClientProxy.Message { Code = code.ToString(), Type = "information", Category = "ProccesPendingFeedBacks", Reason = message });

                        UpdateFileCommentCache("", "", commentfile, result); // update result on cache

                    }

                }
            }

            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("ProccesPendingFeedBacks", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("ProccesPendingFeedBacks", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("ProccesPendingFeedBacks", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

            }

        }


        private void UpdateFileCommentCache(string ProblemNumber,string CaseNumber, FeedBackFileDomain commentfile, ProxyResult result, bool addattempt = true, string message = "")
        {
            byte? filemove = CommonCore.FILE_NOT_MOVE;

            string TransactionId;
            DateTime TransactionDate;
            int attempts = commentfile.Attempts;
            byte? processed = 0;
            if (addattempt) attempts = commentfile.Attempts + 1;

            TransactionId = result.FeedBack != null ? result.FeedBack.CommonArea != null ? result.FeedBack.CommonArea.TransactionId : "NotAvailable" : "NotAvailable";
            TransactionDate = result.FeedBack != null ? result.FeedBack.CommonArea != null ? result.FeedBack.CommonArea.TransactionDate.DateTime : DateTime.Now : DateTime.Now;

            FileStatus statusfile;
            string casenumber;
            DateTime? DateNextAttempt = null;

            if (result.Code.Equals(System.Net.HttpStatusCode.Created))
            {
                statusfile = FileStatus.Dispached;
                message = string.IsNullOrEmpty(message) ? $"El Comentario para el N. de caso {result.FeedBack.CaseNumber} fue enviado satisfactoriamente" : message;
                casenumber = result.CaseCreate != null ? result.CaseCreate.CaseNumber : "";
                processed = 1;
            }
            else
            {
                statusfile = (attempts <= TotalAttemps ? FileStatus.TryAgain : FileStatus.Quarantine);
                processed = (byte?)(statusfile.Equals(FileStatus.Quarantine) ? 1 : 0);
                DateNextAttempt = DateTime.Now.AddSeconds(SecondsWait);
                message = string.IsNullOrEmpty(message) ? $"Petición no creada, intento {attempts} de {TotalAttemps}, proximo intento en {SecondsWait} segundos." : message;
            }

            commentfile.Status = (int)statusfile;
            commentfile.NoTicket = ProblemNumber;
            commentfile.Response = (int)result.Code;
            commentfile.CaseNumber = CaseNumber;
            commentfile.TransactionId = TransactionId;
            commentfile.TransactionDate = TransactionDate;
            commentfile.Attempts = attempts;
            commentfile.FileResponseCreated = CommonCore.FILE_RESPONSE_NO_CREATE;
            commentfile.DateResponse = DateTime.Now;
            commentfile.DateNextAttempt = DateNextAttempt;
            commentfile.Message = message;
            commentfile.Processed = processed;
            commentfile.FileMove = filemove;

            // Mod ticket Status
            var resultrepo = _FeedBackRepoCore.ModifyFeedBack(commentfile);

            /*

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

            */

            if (resultrepo == CacheStatus.Create) this._logger.SuccessAudit("UpdateFileCommentCache", $"Cache updated of file: {commentfile.FileName} - Attempt: { attempts }", 100);

        }


        private CommentData ParseFeedBackData(string filePath)
        {
            var source = utils.GetFileContentbyKeyPair(filePath);
            var CaseNumber = source[CASENUMBERFIELD];
            var Body = source[BODYFIELD];
            var Nticket = source[NTICKETFIELD];

            // Create Case Data 
            CommentData retval = _proxyCore.getCommentData(Nticket,CaseNumber, Body);
            
            return retval;
        }


        public bool RegisterFileFeedBacktoCache(string path)
        {

            var file = new FileInfo(path);

            var status = utils.FileIsEmpty(path) ? FileStatus.Empty : utils.IsFileReady(path) ? FileStatus.Available : FileStatus.Busy;
            FeedBackFileDomain fileComment = new FeedBackFileDomain()
            {
                FileName = file.Name,
                FullPath = file.FullName,
                DateCreate = file.CreationTime,
                DateModified = file.LastWriteTime,
                Length = file.Length,
                Status = (int)status,
                DateNextAttempt = DateTime.Now,
                FileMove = CommonCore.FILE_NOT_MOVE,
                Processed = CommonCore.FILE_NOT_PROCESSED
            };

            var result = _FeedBackRepoCore.RegisterCommentFile(fileComment);

            if (result == CacheStatus.Conflict) UpdateFileCommentCache(path);

            return true;
        }


        public bool UpdateFileCommentCache(string path)
        {
            bool retval = false;

            if (!utils.FileIsEmpty(path))
            {
                var file = new FileInfo(path);
                var domainfile = _FeedBackRepoCore.FindCommentFile(file.Name);
                if (domainfile != null && (file.Length != domainfile.Length))
                {
                    // utils.IsFileReady(e.FullPath);

                    domainfile.Status = (int)(utils.IsFileReady(path) ? FileStatus.Available : FileStatus.Busy);
                    domainfile.Length = file.Length;
                    domainfile.FullPath = file.FullName;
                    domainfile.DateNextAttempt = DateTime.Now;

                    var result = _FeedBackRepoCore.ModifyFeedBack(domainfile);

                    retval = (result == CacheStatus.Create);

                }

            }

            return retval;
        }

        public void AttachFilesCache()
        {


        }

        public void CreateResponseFileList()
        {
            try
            {
                // ************** CREATE RESPONSE FILES *********************

                var filecache_fr = _FeedBackRepoCore.GetFeedBacktoProcessResponse();

                foreach (FeedBackFileDomain feedback in filecache_fr)
                {
                    string fullpath = string.Concat(this.ResponsePath, feedback.NoTicket, ".txt");

                    var result = CreateFeedBackResponseFile(fullpath, feedback);

                    feedback.FileResponseCreated = result ? CommonCore.FILE_RESPONSE_CREATE : CommonCore.FILE_RESPONSE_NO_CREATE;
                    feedback.FullPathResponse = result ? fullpath : "";
                    var resultrepo = _FeedBackRepoCore.ModifyFeedBack(feedback);

                    var msgresult = resultrepo == CacheStatus.Create ? $"File Response of {feedback.NoTicket} created successful" : $"Error of create fileresponse of {feedback.NoTicket}";
                    var typeLog = resultrepo == CacheStatus.Create ? EventLogEntryType.Information : EventLogEntryType.Error;

                    this._logger.WriteLog("CreateResponseFileList", typeLog, msgresult, 100);

                }
            }

            catch (Exception ex)
            {

                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("CreateResponseFileList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("CreateResponseFileList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("CreateResponseFileList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

            }

        }


        public bool CreateFeedBackResponseFile(string fullpath, FeedBackFileDomain feedback)
        {

            List<string> lines = new List<string>() { string.Join("|", "NoTicket", feedback.NoTicket),
                                                          string.Join("|", "Respuesta",  feedback.Response),
                                                          string.Join("|", "CaseNumber", feedback.CaseNumber),
                                                          string.Join("|", "Message", feedback.Message),
                                                          string.Join("|", "DateResponse",  (feedback.DateResponse ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")),
                                                          string.Join("|", "TransactionId", feedback.TransactionId),
                                                          string.Join("|", "TransactionDate",  (feedback.TransactionDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"))
                                                    };

            return utils.createfile(fullpath, lines);

        }


        public void MoveFilesProcess()
        {
            try
            {
                var filecache = _FeedBackRepoCore.GetFeedBacktoMoveList();
                //   var filelistfrompath = utils.GetFileList(this.PendingPath, Extension).ToList();
                // var file_diff = filecache == null ? filelistfrompath.ToList() : filelistfrompath.Where(p => (filecache.Select(s => s.FileName)).Contains(p.Name)).ToList();

                foreach (FeedBackFileDomain fileCache in filecache)
                {
                    //  var ticketFile = this.TicketRepoCore.GetFileTicket(fileCache.Name);

                    var destiny_path = fileCache.Status == (int)FileStatus.Dispached ? DispatchedPath : QuarantinePath;

                    var file = new FileInfo(fileCache.FullPath);

                    if (file.Exists)
                    {
                        var filepath_destiny = String.Concat(destiny_path, fileCache.FileName);

                        file.MoveTo(filepath_destiny);
                        // update cache
                        fileCache.FileMove = CommonCore.FILE_MOVE;
                        fileCache.FullPath = filepath_destiny;
                        this._logger.Info("MoveFilesProcess", $"{fileCache.FileName} has moved to {destiny_path}", 100);
                    }
                    else
                    {
                        fileCache.FileMove = CommonCore.FILE_NOT_EXIST;
                        this._logger.Warning("MoveFilesProcess", $"{fileCache.FileName} not exist on path {fileCache.FullPath}", 100);
                    }

                    var result = this._FeedBackRepoCore.ModifyFeedBack(fileCache);

                }

            }

            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("MoveFilesProcess", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("MoveFilesProcess", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("MoveFilesProcess", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

            }
        }
    }
}
