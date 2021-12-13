using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domain
{
    public class InBoundFileDomain
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public System.DateTime DateReg { get; set; }
        public string TimeReg { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
        public int Processed { get; set; }
        public Nullable<System.DateTime> DateProcessed { get; set; }

    }
}
