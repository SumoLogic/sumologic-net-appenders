using Microsoft.Extensions.Options;
using SumoLogic.Logging.Common.Log;

namespace SumoLogic.Logging.AspNetCoreLogging
{
    public class SumoLogicCoreLoggingOptions : IOptions<SumoLogicCoreLoggingOptions>
    {
        public string SourceName { get; set; } = "AspNetCore-SumoObject";
        public long Timeout { get; set; } = 60000; 
        public SumoLogic.Logging.Common.Log.ILog LogLog { get; set; } = new DummyLog();

        public SumoLogicCoreLoggingOptions Value => this;
    }
}