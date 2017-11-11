using System;
using System.Collections.Generic;
using System.Text;

namespace SumoLogic.Logging.AspNetCoreLogging
{
    public class NoopDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
