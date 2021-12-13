﻿using NetLogger.Implementation;
using Service.Domian.Implementation;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core
{
    public class TicketRepositoryCore
    {

        private Logger _logger = null;

        private byte FILE_RESPONSE_NO_CREATE = 0;
        private byte FILES_NOT_MOVE = 0;

        public TicketRepositoryCore(Logger logger)
        {
            
            _logger = logger;

        }

       
        public Status RegisterTicketFile(TicketFileDomain ticketfiledomain)
        {
           
            try
            {
                TicketFileRepository ticketfile = new TicketFileRepository();

                if (! ticketfile.exists(ticketfiledomain.FileName))
                {
                    var created = ticketfile.Create(ticketfiledomain);
                    ticketfile.Dispose();

                    var type = created != 0 ? EventLogEntryType.Information : EventLogEntryType.Error;
                    var message = created != 0 ? "Registro de FileTicket satisfactoriamente" : "Conflicto al registrar el fileticket";

                    _logger.WriteLog("RegisterTicketFile", type, message, 100);

                    return created != 0 ? Status.Create : Status.Conflict;
                }
                else
                {
                    ticketfile.Dispose();
                   
                    _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Warning, $"No se puede registrar el fileTicket, {ticketfiledomain.FileName} ya fue previamente registrado", 100);

                    return Status.Conflict;
                }

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);
             
                _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Error,"Error: "+ ex.Message, 100);
                _logger.WriteLog("RegisterTicketFile", EventLogEntryType.Error, "InnerException: " +(ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("RegisterTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return Status.InternalError;
            }

        }

        public Status ModifyTicketFile(TicketFileDomain ticketfiledomain)
        {
          
            try
            {
                TicketFileRepository ticketfile = new TicketFileRepository();

                if (ticketfile.exists(ticketfiledomain.FileName))
                {
                    var created = ticketfile.Modify(ticketfiledomain);
                    ticketfile.Dispose();

                    var type = created != 0 ? EventLogEntryType.Information : EventLogEntryType.Warning;
                    var message = created != 0 ? $"FileTicket {ticketfiledomain.FileName} actualizado satisfactoriamente" :$"Conflicto al actualizar el FileTicket {ticketfiledomain.FileName}";

                    _logger.WriteLog("ModifyTicketFile", type, message, 100);

                    return created != 0 ? Status.Create : Status.Conflict;

                }
                else
                {
                    ticketfile.Dispose();
                
                    _logger.WriteLog("ModifyTicketFile", EventLogEntryType.Warning, $"No se puede actualizar el fileTicket ,{ticketfiledomain.FileName} no se encuentra registrado", 100);

                    return Status.Conflict;

                }

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("ModifyTicketFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("ModifyTicketFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("ModifyTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return Status.InternalError;
            }
        }

        public TicketFileDomain FindTicketFile(string filename)
        {

            try
            {
                TicketFileRepository ticketfile = new TicketFileRepository();

                if (ticketfile.exists(filename))
                {
                    var data = ticketfile.Find(filename);
                    ticketfile.Dispose();

                    var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                    var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                    _logger.WriteLog("FindTicketFile", type, message, 100);

                    return data;

                }
                else
                {
                    ticketfile.Dispose();

                    _logger.WriteLog("FindTicketFile", EventLogEntryType.Warning, $"{filename} no se encuentra registrado", 100);

                    return null;

                }

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("FindTicketFile", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("FindTicketFile", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("FindTicketFile", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return null;

            }
        }



        public List<TicketFileDomain> GetTicketFiletoMoveList()
        {
            try
            {
                var statusfileLts = new List<int>() { (int)StatusFile.Dispached, (int)StatusFile.Quarantine };

                TicketFileRepository ticketfile = new TicketFileRepository();

                var destination = ticketfile.ReadAll(statusfileLts).Where(p => p.FileMove == FILES_NOT_MOVE).ToList();

                ticketfile.Dispose();


                //var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                //var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                //_logger.WriteLog("RepositoryCore", type, message, 100);

                return destination;

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("GetTicketFiletoMove", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return null;

            }
        }


        public List<TicketFileDomain> GetTicketFiletoProcessResponse()
        {
            try
            {
               
                var statusfileLts = new List<int>() { (int)StatusFile.Dispached, (int)StatusFile.Quarantine };

                TicketFileRepository ticketfile = new TicketFileRepository();

                var destination = ticketfile.ReadAll(statusfileLts).Where(p => p.FileResponseCreated == FILE_RESPONSE_NO_CREATE).ToList();

                ticketfile.Dispose();


                //var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                //var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                //_logger.WriteLog("RepositoryCore", type, message, 100);

                return destination;

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("GetTicketFiletoProcessResponse", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("GetTicketFiletoProcessResponse", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("GetTicketFiletoProcessResponse", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return null;

            }
        }



        public List<TicketFileDomain> GetTicketFileList()
        {

            try
            {
                TicketFileRepository ticketfile = new TicketFileRepository();

                var destination = ticketfile.ReadAll();

                ticketfile.Dispose();


                //var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                //var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                //_logger.WriteLog("RepositoryCore", type, message, 100);

                return destination;

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("GetTicketFileList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return null;

            }
        }



        public List<TicketFileDomain> GetTicketFiletoAttempt(List<int> statuslts)
        {

            try
            {
                TicketFileRepository ticketfile = new TicketFileRepository();

                var data = ticketfile.ReadAll(statuslts);

                var destination = data.Where(p => p.DateNextAttempt <= DateTime.Now).ToList();

                ticketfile.Dispose();


                //var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                //var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                //_logger.WriteLog("RepositoryCore", type, message, 100);

                return destination;

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("GetTicketFileList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("GetTicketFileList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return null;

            }
        }


        public enum Status
        {
            Found = 200,
            Create = 201,
            NotFound = 404,
            Conflict = 409,
            InternalError = 500
        }


        public enum StatusFile
        {
            Empty = 10,
            Busy = 20,
            Available = 30,
            Dispached = 40,
            Quarantine = 50,
            TryAgain = 60

        }

    }
}