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
    public class AtmRepositoryCore
    {

        private ILogger _logger = null;

        public AtmRepositoryCore(ILogger logger)
        {

            _logger = logger;

        }


        public AtmCatDomain GetAtm(string id)
        {
            AtmCatDomain result;
            using (var atmrepo = new AtmCatRepository())
            {
                try
                {
                    if (atmrepo.exists(id))
                    {
                        var data = atmrepo.Find(id);

                        var type = data != null ? EventLogEntryType.SuccessAudit : EventLogEntryType.Warning;
                        var message = data != null ? $"id {id} identificado en catalogo atm" : $"No se identifico el id {id} en catalogo";

                        _logger.WriteLog("GetAtm", type, message, 100);

                        result = data;
                    }
                    else
                    {
                        _logger.WriteLog("GetAtm", EventLogEntryType.Warning, $"id {id} no se encuentra registrado", 100);

                        result = null;
                    }
                }
                catch (Exception ex)
                {
                    StackTrace trace = new StackTrace(ex, true);

                    _logger.WriteLog("GetAtm", EventLogEntryType.Error, "Error: " + ex.Message, 100);
                    _logger.WriteLog("GetAtm", EventLogEntryType.Error, "InnerException: " + (ex.InnerException != null ? ex.InnerException.Message : ""), 100);
                    _logger.WriteLog("GetAtm", EventLogEntryType.FailureAudit, "Stacktrace: " + trace.ToString(), 100);

                    result = null;
                }
            }
            return result;
        }

    }
}
