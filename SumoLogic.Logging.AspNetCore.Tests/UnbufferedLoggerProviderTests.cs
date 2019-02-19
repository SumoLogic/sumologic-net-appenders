using Microsoft.Extensions.Logging;
using SumoLogic.Logging.Common.Sender;
using System;
using System.Threading;
using Xunit;

namespace SumoLogic.Logging.AspNetCore.Tests
{
    /// <summary>
    /// Sumo Logic Logger Provider test implementation.
    /// </summary>
    [Collection("Logger provider tests")]
    public class UnbufferedLoggerProviderTests: IDisposable
    {
        public UnbufferedLoggerProviderTests()
        {
            _messagesHandler = new MockHttpMessageHandler();

            _provider = new LoggerProvider(new LoggerOptions()
            {
                Uri = "http://www.fakeadress.com",
                SourceName = "LoggerProviderTestSourceName",
                SourceCategory = "LoggerProviderTestSourceCategory",
                SourceHost = "LoggerProviderTestSourceHost",
                IsBufferred = false,
                HttpMessageHandler = _messagesHandler,
            });

            _logger = _provider.CreateLogger("OverriddenCategory");
        }

        private MockHttpMessageHandler _messagesHandler;

        private LoggerProvider _provider;

        private ILogger _logger;

        /// <summary>
        /// Test logging of a single message.
        /// </summary>
        [Fact]
        public void SingleMessageTest()
        {
            _logger.LogInformation("This is a message");
            Assert.Equal(1, _messagesHandler.ReceivedRequests.Count);
            Assert.Equal($"This is a message{Environment.NewLine}", _messagesHandler.LastReceivedRequest.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Test logging of multiple messages.
        /// </summary>
        [Fact]
        public void MultipleMessagesTest()
        {
            var numMessages = 50;
            for (var i = 0; i < numMessages / 5; i++)
            {
                _logger.LogDebug(i.ToString());
                _logger.LogInformation(i.ToString());
                _logger.LogWarning(i.ToString());
                _logger.LogError(i.ToString());
                _logger.LogCritical(i.ToString());
            }

            Assert.True(_messagesHandler.ReceivedRequests.Count > 5);
        }

        /// <summary>
        /// Test do not logging on the trace level.
        /// </summary>
        [Fact]
        public void NoLogOnTheLevelTraceTest()
        {
            _logger.LogTrace("This is message");
            Assert.Equal(0, _messagesHandler.ReceivedRequests.Count);
        }

        /// <summary>
        /// Test logging multiple message and checking the request content.
        /// </summary>
        [Fact]
        public void CheckedRequestContentTest()
        {
            _logger.LogDebug("This is first message");
            _logger.LogInformation("This is second message");
            _logger.LogWarning("This is third message");
            _logger.LogError("This is fourth message");
            _logger.LogCritical("This is fifh message");
            Assert.Equal($"This is fifh message{Environment.NewLine}", _messagesHandler.ReceivedRequests[4].Content.ReadAsStringAsync().Result);
            Assert.Equal($"This is fourth message{Environment.NewLine}", _messagesHandler.ReceivedRequests[3].Content.ReadAsStringAsync().Result);
            Assert.Equal($"This is third message{Environment.NewLine}", _messagesHandler.ReceivedRequests[2].Content.ReadAsStringAsync().Result);
            Assert.Equal($"This is second message{Environment.NewLine}", _messagesHandler.ReceivedRequests[1].Content.ReadAsStringAsync().Result);
            Assert.Equal($"This is first message{Environment.NewLine}", _messagesHandler.ReceivedRequests[0].Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _provider.Dispose();
                _messagesHandler.Dispose();
            }
        }

    }
}
