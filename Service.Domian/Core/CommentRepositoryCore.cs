using NetLogger.Implementation;
using Service.Domian.Implementation;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core
{
    public class CommentRepositoryCore
    {
        private Logger _logger = null;

        private byte COMMENT_NOT_PROCESSED = 0;

        private byte FILE_RESPONSE_NO_CREATE = 0;
        private byte FILES_NOT_MOVE = 0;

        public CommentRepositoryCore(Logger logger)
        {

            _logger = logger;

        }


        public CacheStatus RegisterCommentFile(CommentFileDomain commentfiledomain)
        {

            try
            {
                CommentFileRepository CommentRepo = new CommentFileRepository();

                if (!CommentRepo.exists(commentfiledomain.FileName))
                {
                    var created = CommentRepo.Create(commentfiledomain);
                    CommentRepo.Dispose();

                    var type = created != 0 ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit;
                    var message = created != 0 ? $"{commentfiledomain.FileName} registration successful" : $"Conflict registering the {commentfiledomain.FileName}";

                    _logger.WriteLog("RegisterCommentFile", type, message, 100);

                    return created != 0 ? CacheStatus.Create : CacheStatus.Conflict;
                }
                else
                {
                    CommentRepo.Dispose();

                    _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Warning, $"No se puede registrar el fileTicket, {commentfiledomain.FileName} ya fue previamente registrado", 100);

                    return CacheStatus.Conflict;
                }

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("RegisterCommentFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("RegisterCommentFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return CacheStatus.InternalError;
            }

        }



    }
}
