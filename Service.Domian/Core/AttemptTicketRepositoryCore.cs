using NetLogger.Implementation;
using Service.DataAccess.ORM;
using Service.Domian.Implementation;
using Service.Domian.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Core
{
    public class AttemptTicketRepositoryCore
    {

        private Logger _logger = null;

        public AttemptTicketRepositoryCore(Logger logger)
        {

            _logger = logger;

        }

        public Status RegisterTicketAttempt(AttemptTicketDomain ticketattemptdomain)
        {

            try
            {
                AttemptTicketRepository ticketattempt = new AttemptTicketRepository();

                var created = ticketattempt.Create(ticketattemptdomain);
                ticketattempt.Dispose();

                var type = created != 0 ? EventLogEntryType.Information : EventLogEntryType.Error;
                var message = created != 0 ? "Registro de intento satisfactorio" : "Conflicto al registrar el intento";

                _logger.WriteLog("AttemptTicketRepositoryCore", type, message, 100);

                return created != 0 ? Status.Create : Status.Conflict;

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    string message = $"Entity of type \"{eve.Entry.Entity.GetType().Name}\" in state \"{eve.Entry.State}\" has the following validation errors:";
                    _logger.WriteLog("RegisterTicketAttempt", EventLogEntryType.Error, message,100);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);

                        string message2 = $"Entity of type \"{eve.Entry.Entity.GetType().Name}\" in state \"{eve.Entry.State}\" has the following validation errors:";
                        _logger.WriteLog("RegisterTicketAttempt", EventLogEntryType.Error, message2, 100);

                    }
                }
                return Status.InternalError;
            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("RegisterTicketAttempt", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("RegisterTicketAttempt", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.InnerException.ToString() : ""), 100);
                _logger.WriteLog("RegisterTicketAttempt", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                return Status.InternalError;
            }

        }


        public List<AttemptTicketDomain> TicketAttemptList(string NTicket)
        {

            try
            {
                AttemptTicketRepository ticketattempt = new AttemptTicketRepository();

                var data = ticketattempt.ReadAll(NTicket);
                ticketattempt.Dispose();
                //var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                //var message = data != null ? "FileTicket identificado " : "No se identifico el fileticket";

                //_logger.WriteLog("RepositoryCore", type, message, 100);

                return data;

            }
            catch (Exception ex)
            {
                StackTrace trace = new StackTrace(ex, true);

                _logger.WriteLog("TicketAttemptList", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                _logger.WriteLog("TicketAttemptList", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                _logger.WriteLog("TicketAttemptList", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

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

    }
}
