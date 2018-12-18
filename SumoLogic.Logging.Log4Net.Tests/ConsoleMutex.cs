using System;

namespace SumoLogic.Logging.Log4Net.Tests
{
    /// <summary>
    /// Set mutex on different test cases using Console.stdout
    /// </summary>
    class ConsoleMutex
    {
        public static Object mutex = new object();
    }
}
