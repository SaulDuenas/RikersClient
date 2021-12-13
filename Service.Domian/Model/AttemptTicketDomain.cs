using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Model
{
    public class AttemptTicketDomain
    {
        public int Id { get; set; }
        public string NoTicket { get; set; }
        public int NAttempt { get; set; }
        public int Response { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public System.DateTime DateResponse { get; set; }

    }

}
