using Microsoft.Extensions.Logging;
using SumoLogic.Logging.Common.Log;

namespace SumoLogic.Logging.AspNetCore
{
    /// <summary>
    /// ILog wrapper of .NET Core ILogger
    /// </summary>
    public class LoggerLog: ILog
    {
        private ILogger logger;

        public LoggerLog(ILogger logger)
        {
            this.logger = logger;
        }

        bool ILog.IsTraceEnabled => true;

        bool ILog.IsDebugEnabled => true;

        bool ILog.IsInfoEnabled => true;

        bool ILog.IsWarnEnabled => true;

        bool ILog.IsErrorEnabled => true;

        void ILog.Debug(string msg)
        {
            logger.Log(LogLevel.Debug, msg);
        }

        void ILog.Error(string msg)
        {
            logger.Log(LogLevel.Error, msg);
        }

        void ILog.Info(string msg)
        {
            logger.Log(LogLevel.Information, msg);
        }

        void ILog.Trace(string msg)
        {
            logger.Log(LogLevel.Trace, msg);
        }

        void ILog.Warn(string msg)
        {
            logger.Log(LogLevel.Warning, msg);
        }
    }
}
