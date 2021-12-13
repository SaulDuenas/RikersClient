using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities
{
        public partial class ResponseMessage
        {
            [JsonProperty("transactionId")]
            public string TransactionId { get; set; }

            [JsonProperty("transactionDate")]
            public DateTimeOffset TransactionDate { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("node")]
            public string Node { get; set; }

            [JsonProperty("messages")]
            public Message[] Messages { get; set; }

            [JsonProperty("common area")]
            public CommonArea CommonArea { get; set; }

    }

}
