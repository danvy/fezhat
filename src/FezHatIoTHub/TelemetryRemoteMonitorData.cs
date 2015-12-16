using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FezHatIoTHub
{
    public class TelemetryRemoteMonitorData
    {
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double? ExternalTemperature { get; set; }
    }
}
