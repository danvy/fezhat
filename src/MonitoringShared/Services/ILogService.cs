using System;
using System.Collections.Generic;
using System.Text;

namespace MonitoringShared.Services
{
    public interface ILogService
    {
        void WriteLine(string value);
    }
}
