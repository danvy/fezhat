using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
