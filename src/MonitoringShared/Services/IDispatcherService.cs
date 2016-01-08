using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;

namespace Danvy.Services
{
    public interface IDispatcherService
    {
        IAsyncAction RunAsync(Action action);
    }
}
