using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RikersProxy.Entities.General
{
    public partial class Asset
    {
        [JsonProperty("ibmMachineType")]
        public string IbmMachineType { get; set; }

        [JsonProperty("ibmMachineModel")]
        public string IbmMachineModel { get; set; }
        
        [JsonProperty("serial")]
        public string Serial { get; set; }

    }
}
