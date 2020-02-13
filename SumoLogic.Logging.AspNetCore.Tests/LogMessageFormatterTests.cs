using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SumoLogic.Logging.AspNetCore.Tests
{
    [Collection("Logger provider tests")]
    public class LogMessageFormatterTests
    {
        Func<string, Exception, string, LogLevel, IDictionary<string, object>, string> _defaultFormatter = new LoggerOptions().MessageFormatterFunc;

        private string TrimDate(string message)
        {
            var logLevelIndex = message.IndexOf("[");

            return message.Substring(logLevelIndex);
        }

        [Theory]
        [InlineData("msg", LogLevel.Information, false)]
        [InlineData("", LogLevel.Error, true)]
        [InlineData("msg", LogLevel.Error, true)]
        public void DefaultFormatterDoesNotThrowTest(string message, LogLevel level, bool hasException)
        {
            Exception ex = hasException ? new Exception("Exception message") : null;
            string expectedResult = $"[{level}] {message} {ex}".TrimEnd();
            var actual = TrimDate(_defaultFormatter(message, ex, null, level, null));

            Assert.Equal(expectedResult, actual);
        }
    }
}
