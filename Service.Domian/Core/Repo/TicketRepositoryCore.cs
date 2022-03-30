using NetLogger.Implementation;
using Service.Domian.Implementation;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core.Repo
{
    public class TicketRepositoryCore
    {

        private ILogger _logger = null;

        public TicketRepositoryCore(ILogger logger)
        {
            
            _logger = logger;

        }

       
        public CacheStatus RegisterTicketFile(TicketFileDomain ticketfiledomain)
        {
            CacheStatus result;
            using (var ticketfile = new TicketFileRepository()) 
            {

                try
                {
                    if (!ticketfile.exists(ticketfiledomain.FileName))
                    {
                        var created = ticketfile.Create(ticketfiledomain);
                      
                        var type = created != 0 ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit;
                        var message = created != 0 ? $"{ticketfiledomain.FileName} registration successful" : $"Conflict registering the {ticketfiledomain.FileName}";

                        _logger.WriteLog("RegisterTicketFile", type, message, 100);

                        result = created != 0 ? CacheStatus.Create : CacheStatus.Conflict;
                    }
                    else
                    {
                        _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Warning, $"No se puede registrar el fileTicket, {ticketfiledomain.FileName} ya fue previamente registrado", 100);

                        result = CacheStatus.Conflict;
                    }

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("RegisterTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = CacheStatus.InternalError;
                }
            }

            return result;
        }

        public CacheStatus ModifyTicketFile(TicketFileDomain ticketfiledomain)
        {
            CacheStatus result;
            using (var ticketfile = new TicketFileRepository())
            {

                try
                {

                    if (ticketfile.exists(ticketfiledomain.FileName))
                    {
                        var created = ticketfile.Modify(ticketfiledomain);

                        var type = created != 0 ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                        var message = created != 0 ? $"FileTicket {ticketfiledomain.FileName} actualizado satisfactoriamente" : $"Conflicto al actualizar el FileTicket {ticketfiledomain.FileName}";

                        _logger.WriteLog("ModifyTicketFile", type, message, 100);

                        result = created != 0 ? CacheStatus.Create : CacheStatus.Conflict;

                    }
                    else
                    {
                      
                        _logger.Warning("ModifyTicketFile", $"No se puede actualizar el fileTicket ,{ticketfiledomain.FileName} no se encuentra registrado", 100);

                        result = CacheStatus.Conflict;

                    }

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("ModifyTicketFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("ModifyTicketFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                    _logger.WriteLog("ModifyTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = CacheStatus.InternalError;
                }

            }
            return result;
        }

        public TicketFileDomain GetFileTicket(string filename)
        {
            TicketFileDomain result;
            using (var ticketfile = new TicketFileRepository())
            {
                try
                {
                    if (ticketfile.exists(filename))
                    {
                        var data = ticketfile.Find(filename);

                        var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                        var message = data != null ? $"{filename} identificado en cache" : $"No se identifico el fileticket {filename}";

                        _logger.WriteLog("FindTicketFile", type, message, 100);

                        result = data;

                    }
                    else
                    {
                        _logger.WriteLog("FindTicketFile", EventLogEntryType.Warning, $"{filename} no se encuentra registrado", 100);

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

        public List<TicketFileDomain> GetTicketFiletoMoveList()
        {
            List<TicketFileDomain> result = null;
            using (var ticketfile = new TicketFileRepository())
            {
                try
                {
                    var statusfileLts = new List<int>() { (int)FileStatus.Dispached, (int)FileStatus.Quarantine };

                    var destination = ticketfile.ReadAll(statusfileLts).Where(p => p.FileMove == CommonCore.FILE_NOT_MOVE).ToList();

                    result = destination;

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;

                }

            }

            return result;
        }

        public List<TicketFileDomain> GetFileTicketstoPendingProcess()
        {
            var statuslts = new List<int>() { (int)FileStatus.Available, (int)FileStatus.TryAgain };

            var destiny = GetFileTicketsByStatus(statuslts);

            return destiny.Where(p => p.DateNextAttempt <= DateTime.Now && p.Processed == CommonCore.FILE_NOT_PROCESSED).ToList();
        }

        public List<TicketFileDomain> getTicketFilesNotAvailable()
        {
            var statuslts = new List<int>() { (int)FileStatus.Available, (int)FileStatus.Busy, (int)FileStatus.Empty };
            return GetFileTicketsByStatus(statuslts);
        }

        public List<TicketFileDomain> GetFileTicketstoProcessResponse()
        {
            var statuslts = new List<int>() { (int)FileStatus.Dispached, (int)FileStatus.Quarantine, (int)FileStatus.TryAgain };

            return GetFileTicketsByStatus(statuslts).Where(p => p.FileResponseCreated == CommonCore.FILE_RESPONSE_NO_CREATE).ToList();
        }

        public List<TicketFileDomain> GetAllFileTickets()
        {
            List<TicketFileDomain> result = null;
            using (var ticketfile = new TicketFileRepository())
            {
                try
                {

                    var destination = ticketfile.ReadAll();

                    result = destination;

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;

                }
            }

            return result;
        }

        public List<TicketFileDomain> GetFileTicketsByStatus(List<int> status)
        {
            List<TicketFileDomain> result = null;
            using (var ticketfile = new TicketFileRepository())
            {
                try
                {
                    var destination = ticketfile.ReadAll(status);

                    result = destination;

                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("GetTicketFileList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;

                }

            }
            return result;
        }

    }
}
