using System.Runtime.Serialization;

namespace FezHatIoTHub
{
    public class DeviceCommandParameter
    {
        public string Name { get; set;  }
        public DeviceCommandParameterType Type { get; set; }
    }
}