using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RikersProxy.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities
{        
   public partial class CaseData
        {
            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("customerProblemNumber")]
            public string CustomerProblemNumber { get; set; }

            [JsonProperty("customer")]
            public Customer Customer { get; set; }

            [JsonProperty("caseContact")]
            public CaseContact CaseContact { get; set; }

            [JsonProperty("asset")]
            public Asset Asset { get; set; }
        }
   
}
