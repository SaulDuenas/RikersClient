using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities.General
{
    public partial class Customer
    {
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("ibmCustomerNumber")]
        public string IbmCustomerNumber { get; set; }
    }
}
