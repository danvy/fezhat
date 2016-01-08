using System.Diagnostics;
using Danvy.Services;

namespace MonitoringShared.Services
{
    public class DebugLogService : ILogService
    {
        public void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }
    }
}
