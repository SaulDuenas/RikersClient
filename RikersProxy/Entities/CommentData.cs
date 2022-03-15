using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities
{
    public partial class CommentData
    {
        [JsonProperty("caseNumber")]
        public string CaseNumber { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
