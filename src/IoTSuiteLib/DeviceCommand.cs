using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IoTSuiteLib
{
    public class DeviceCommand
    {
        public DeviceCommand()
        {
            Parameters = new List<DeviceCommandParameter>();
        }
        public string Name { get; set; }
        public string MessageId { get; set; }
        public string CreatedTime { get; set; }
        [JsonIgnore]
        public List<DeviceCommandParameter> Parameters { get; set; }
    }
}
