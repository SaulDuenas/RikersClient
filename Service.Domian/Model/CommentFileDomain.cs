using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Model
{
   public class CommentFileDomain
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public Nullable<System.DateTime> DateCreate { get; set; }
        public Nullable<System.DateTime> DateModified { get; set; }
        public Nullable<long> Length { get; set; }
        public Nullable<int> Status { get; set; }
        public string NoTicket { get; set; }
        public string CaseNumber { get; set; }
        public int Attempts { get; set; }
        public Nullable<int> Response { get; set; }
        public string TransactionId { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public Nullable<System.DateTime> DateResponse { get; set; }
        public Nullable<byte> FileResponseCreated { get; set; }
        public string FullPathResponse { get; set; }

    }
}
