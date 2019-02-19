using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumoLogic.Logging.Common.Log;
using SumoLogic.Logging.Common.Queue;
using SumoLogic.Logging.Common.Sender;
using System;
using System.Threading;

namespace SumoLogic.Logging.AspNetCore
{
    [ProviderAlias("SumoLogic")]
    public class LoggerProvider : ILoggerProvider
    {
        public LoggerOptions LoggerOptions { get; private set; }

        private ILog DebuggingLogger { get; set; }

        private SumoLogicMessageSender SumoLogicMessageSender { get; set; }

        private Timer flushBufferTimer = null;

        private SumoLogicMessageSenderBufferFlushingTask flushBufferTask = null;

        private volatile BufferWithEviction<string> messagesQueue = null;

        public LoggerProvider(IOptionsMonitor<LoggerOptions> options)
        {
            ReConfig(options.CurrentValue);
        }

        public LoggerProvider(LoggerOptions options)
        {
            ReConfig(options);
        }

        private void ReConfig(LoggerOptions options)
        {
            Dispose();
            if (string.IsNullOrWhiteSpace(options.Uri))
            {
                throw new ArgumentOutOfRangeException(nameof(options.Uri), $"{nameof(options.Uri)} cannot be empty.");
            }
            DebuggingLogger = options.DebuggingLogger != null ? new LoggerLog(options.DebuggingLogger) : null;
            InitSender(options);
            if (options.IsBufferred)
            {
                InitBuffer(options);
            }
            LoggerOptions = options;
        }

        private void InitSender(LoggerOptions options)
        {
            DebuggingLogger?.Debug("InitSender");
            SumoLogicMessageSender = new SumoLogicMessageSender(options.HttpMessageHandler, DebuggingLogger, "asp.net-core-logger")
            {
                Url = new Uri(options.Uri),
                ConnectionTimeout = options.ConnectionTimeout,
                RetryInterval = options.RetryInterval
            };
            DebuggingLogger?.Debug("InitSender::Completed");
        }

        private void InitBuffer(LoggerOptions options)
        {
            DebuggingLogger?.Debug("InitBuffer");

            messagesQueue = new BufferWithFifoEviction<string>(
                options.MaxQueueSizeBytes,
                new StringLengthCostAssigner(),
                DebuggingLogger);

            flushBufferTask = new SumoLogicMessageSenderBufferFlushingTask(
                messagesQueue,
                SumoLogicMessageSender,
                options.MaxFlushInterval,
                options.MessagesPerRequest,
                options.SourceName,
                options.SourceCategory,
                options.SourceHost,
                DebuggingLogger);

            flushBufferTimer = new Timer(
                callback: (s) => flushBufferTask.Run(), 
                state: null, 
                dueTime: TimeSpan.FromMilliseconds(0), 
                period: options.FlushingAccuracy);

            DebuggingLogger?.Debug("InitBuffer::Completed");
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(this, categoryName);
        }

        public void Dispose()
        {
            flushBufferTimer?.Dispose();

            flushBufferTask?.FlushAndSend();

            SumoLogicMessageSender?.Dispose();
        }

        public void WriteLine(String message, String categoryName)
        {
            if (null == message)
            {
                return;
            }

            if (SumoLogicMessageSender == null || !SumoLogicMessageSender.CanTrySend)
            {
                DebuggingLogger?.Warn("Sender is not initialized. Dropping log entry");
                return;
            }

            String line = string.Concat(
                message.TrimEnd(Environment.NewLine.ToCharArray()), 
                Environment.NewLine);

            if (LoggerOptions.IsBufferred)
            {
                messagesQueue.Add(line);
            }
            else
            {
                WriteLineToSumo(line);
            }
        }

        private void WriteLineToSumo(String body)
        {
            SumoLogicMessageSender.TrySend(
                body, 
                LoggerOptions.SourceName,
                LoggerOptions.SourceCategory,
                LoggerOptions.SourceHost);
        }

    }
}
