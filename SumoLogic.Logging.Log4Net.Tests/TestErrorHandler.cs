using System;
using log4net.Core;

namespace SumoLogic.Logging.Log4Net.Tests
{
    /// <summary>
    /// Simple test implementation of Log4Net error handler
    /// </summary>
    public class TestErrorHandler : IErrorHandler
    {
        public System.Collections.Generic.List<string> Errors { get; } = new System.Collections.Generic.List<string>();

        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            Errors.Add(String.Format("{0} {1} {2}", message, e, errorCode));
        }

        public void Error(string message, Exception e)
        {
            Errors.Add(String.Format("{0} {1}", message, e));
        }

        public void Error(string message)
        {
            Errors.Add(message);
        }
    }
}