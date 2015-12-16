using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FezHatIoTHub
{
    public class DeviceMetaData
    {
        public DeviceMetaData()
        {
            Properties = new DeviceProperties();
            Commands = new List<DeviceCommand>();
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public ObjectType ObjectType { get; set; }
        public bool IsSimulatedDevice { get; set; }
        public string Version { get; set; }
        [JsonProperty(PropertyName = "DeviceProperties")]
        public DeviceProperties Properties { get; set; }
        public List<DeviceCommand> Commands { get; set; }
    }
}
