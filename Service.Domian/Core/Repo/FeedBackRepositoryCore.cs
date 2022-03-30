using NetLogger.Implementation;
using Service.Domian.Implementation;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core.Repo
{
    public class FeedBackRepositoryCore
    {
        private ILogger _logger = null;

        public FeedBackRepositoryCore(ILogger logger)
        {

            _logger = logger;

        }


        public List<FeedBackFileDomain> GetFeedBackPendingProcess()
        {
            var stsLts = new List<int>() { (int)FileStatus.Available, (int)FileStatus.TryAgain };
            var destiny = GetFeedBackByStatus(stsLts);

            return destiny.Where(p => p.DateNextAttempt <= DateTime.Now && p.Processed == CommonCore.FILE_NOT_PROCESSED).ToList();
        }


        public List<FeedBackFileDomain> getFeedBackNotAvailable()
        {
            var statuslts = new List<int>() { (int)FileStatus.Available, (int)FileStatus.Busy, (int)FileStatus.Empty };
            return GetFeedBackByStatus(statuslts);
        }

        public List<FeedBackFileDomain> GetFeedBacktoProcessResponse()
        {
            var statuslts = new List<int>() { (int)FileStatus.Dispached, (int)FileStatus.Quarantine, (int)FileStatus.TryAgain };

            return GetFeedBackByStatus(statuslts).Where(p => p.FileResponseCreated == CommonCore.FILE_RESPONSE_NO_CREATE).ToList();
        }


        public List<FeedBackFileDomain> GetFeedBacktoMoveList()
        {
            var statuslts = new List<int>() { (int)FileStatus.Dispached, (int)FileStatus.Quarantine };

            return GetFeedBackByStatus(statuslts).Where(p => p.FileMove == CommonCore.FILE_NOT_MOVE).ToList();
        }


        public CacheStatus RegisterCommentFile(FeedBackFileDomain commentdomain)
        {
            CacheStatus result;

            using (var commentRepo = new CommentFileRepository()) 
            {
                try
                {

                    if (!commentRepo.exists(commentdomain.FileName))
                    {
                        var created = commentRepo.Create(commentdomain);
                      
                        var type = created != 0 ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit;
                        var message = created != 0 ? $"{commentdomain.FileName} registration successful" : $"Conflict registering the {commentdomain.FileName}";

                        _logger.WriteLog("RegisterCommentFile", type, message, 100);

                        result = created != 0 ? CacheStatus.Create : CacheStatus.Conflict;
                    }
                    else
                    {
                        _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Warning, $"No se puede registrar el fileComment, {commentdomain.FileName} ya fue previamente registrado", 100);

                        result = CacheStatus.Conflict;
                    }

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("RegisterCommentFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = CacheStatus.InternalError;
                }
            }

            return result;

        }


        public CacheStatus ModifyFeedBack(FeedBackFileDomain commentdomain)
        {
            CacheStatus result;

            using (var commentRepo = new CommentFileRepository())
            {
                try
                {

                    if (commentRepo.exists(commentdomain.FileName))
                    {
                        var created = commentRepo.Modify(commentdomain);
        
                        var type = created != 0 ? EventLogEntryType.Information : EventLogEntryType.Warning;
                        var message = created != 0 ? $"CommentTicket {commentdomain.FileName} actualizado satisfactoriamente" : $"Conflicto al actualizar el FileComment {commentdomain.FileName}";

                        _logger.WriteLog("ModifyCommentFile", type, message, 100);

                        result = created != 0 ? CacheStatus.Create : CacheStatus.Conflict;

                    }
                    else
                    {
                        
                        _logger.WriteLog("ModifyCommentFile", EventLogEntryType.Warning, $"No se puede actualizar el fileComment ,{commentdomain.FileName} no se encuentra registrado", 100);

                        result = CacheStatus.Conflict;
                    }

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("ModifyCommentFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("ModifyCommentFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("ModifyCommentFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = CacheStatus.InternalError;
                }
            }

            return result;
               
        }


        public FeedBackFileDomain FindCommentFile(string filename)
        {
            FeedBackFileDomain result;

            using (var commentRepo = new CommentFileRepository())
            {
                try
                {

                    if (commentRepo.exists(filename))
                    {
                        var data = commentRepo.Find(filename);

                        var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                        var message = data != null ? "FileComment identificado " : "No se identifico el fileComment";

                        _logger.WriteLog("FindCommentFile", type, message, 100);

                        result = data;

                    }
                    else
                    {

                        _logger.WriteLog("FindCommentFile", EventLogEntryType.Warning, $"{filename} no se encuentra registrado", 100);

                        result = null;

                    }

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("FindTicketFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("FindTicketFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("FindTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;

                }
            }

            return result;
        }


        public List<FeedBackFileDomain> GetFeedBackByStatus(List<int> status)
        {
            List<FeedBackFileDomain> result=null;

            using (var commentRepo = new CommentFileRepository())
            {

                try
                {
                    var destination = commentRepo.ReadAll(status);

                    result = destination;

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("GetFeedBackByStatus", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("GetFeedBackByStatus", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("GetFeedBackByStatus", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;

                }

            }
            
            return result;
        }


    }
}
