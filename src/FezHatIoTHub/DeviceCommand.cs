using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FezHatIoTHub
{
    public class DeviceCommand
    {
        public DeviceCommand()
        {
            Parameters = new List<DeviceCommandParameter>();
        }
        public string Name { get; set; }
        [DataMember(Name = "DeviceParameters", Order = 1)]
        public List<DeviceCommandParameter> Parameters { get; set; }
    }
}
