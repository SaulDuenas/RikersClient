using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAppR2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

          //  var TicketService = new TicketService();
          //  TicketService.OnDebug();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new TicketService()
            };

            ServiceBase.Run(ServicesToRun);
            
        }
    }
}
