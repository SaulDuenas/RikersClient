using Newtonsoft.Json;
using RikersProxy.Entities.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities
{
    public partial class ResponseCaseCreated
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("caseNumber")]
            public string CaseNumber { get; set; }

            [JsonProperty("sourceSystemId")]
            public string SourceSystemId { get; set; }

            [JsonProperty("subject")]
            public string Subject { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("severity")]
            public long Severity { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("liableOrganization")]
            public string LiableOrganization { get; set; }

            [JsonProperty("liableCategory")]
            public string LiableCategory { get; set; }

            [JsonProperty("customerProblemNumber")]
            public string CustomerProblemNumber { get; set; }

            [JsonProperty("reqEntitlementPrecheck")]
            public bool ReqEntitlementPrecheck { get; set; }

            [JsonProperty("customer")]
            public Customer Customer { get; set; }

            [JsonProperty("caseContact")]
            public CaseContact CaseContact { get; set; }

            [JsonProperty("asset")]
            public Asset Asset { get; set; }

            [JsonProperty("common area")]
            public CommonArea CommonArea { get; set; }
    }

}
