using System;
using System.Threading;
using Xunit.Sdk;

namespace SumoLogic.Logging.Common.Tests
{
    public static class TestHelper
    {
        public delegate void VerifierDelegate();

        public static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(10);

        public static TimeSpan DEFAULT_INTERVAL = TimeSpan.FromMilliseconds(100);

        public static void Eventually(TimeSpan timeout, TimeSpan interval, VerifierDelegate verifier)
        {
            DateTime finalTime = DateTime.Now + timeout;
            Exception err = null;
            int tried = 0;
            while (DateTime.Now < finalTime)
            {
                Thread.Sleep(interval);
                try
                {
                    verifier();
                    err = null;
                    return;
                }
                catch(Exception ex)
                {
                    err = ex;
                    tried++;
                }
            }
            if (null != err)
            {
                string message = $"Still get \"{err.GetType().Name}\" after {timeout} and {tried} trys.";
                throw new EventuallyException(message, err);
            }
            
        }

        public static void Eventually(VerifierDelegate verifier)
        {
            Eventually(DEFAULT_TIMEOUT, DEFAULT_INTERVAL, verifier);
        }

        public class EventuallyException: XunitException {
            public EventuallyException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
}
