using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Domian.Dto
{
   public class CaseDataDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string CustomerProblemNumber { get; set; }
        public DtoCustomer Customer { get; set; }
        public CaseContactDto CaseContact { get; set; }
        public AssetDto Asset { get; set; }
        
    }
}
