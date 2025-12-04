using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using SumoLogic.Logging.Serilog.Extensions;

namespace SumoLogic.Logging.Serilog.LoadTest
{
    class Program
    {
        private static readonly string SumoLogicEndpoint = 
            "https://collectors.sumologic.com/receiver/v1/http/YOUR_ENDPOINT_HERE";

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Sumo Logic Serilog Load Test ===");
            Console.WriteLine($"Endpoint: {SumoLogicEndpoint.Substring(0, 50)}...");
            Console.WriteLine();

            // Display menu
            while (true)
            {
                Console.WriteLine("Select Load Test Scenario:");
                Console.WriteLine("1. Sustained Load Test (1000 msgs/sec for 5 minutes)");
                Console.WriteLine("2. Burst Test (10000 msgs in 10 seconds)");
                Console.WriteLine("3. Spike Test (alternating high/low load)");
                Console.WriteLine("4. Stress Test (gradually increasing load)");
                Console.WriteLine("5. Endurance Test (moderate load for 30 minutes)");
                Console.WriteLine("6. Custom Test (configure your own)");
                Console.WriteLine("7. Exit");
                Console.Write("\nEnter choice (1-7): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RunSustainedLoadTest();
                        break;
                    case "2":
                        await RunBurstTest();
                        break;
                    case "3":
                        await RunSpikeTest();
                        break;
                    case "4":
                        await RunStressTest();
                        break;
                    case "5":
                        await RunEnduranceTest();
                        break;
                    case "6":
                        await RunCustomTest();
                        break;
                    case "7":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.\n");
                        break;
                }

                Console.WriteLine("\n" + new string('-', 80) + "\n");
            }
        }

        static async Task RunSustainedLoadTest()
        {
            Console.WriteLine("\n>>> Running Sustained Load Test <<<");
            Console.WriteLine("Target: 1000 messages/second for 5 minutes\n");

            var config = new LoadTestConfig
            {
                TestName = "Sustained Load",
                TargetMessagesPerSecond = 1000,
                DurationSeconds = 300, // 5 minutes
                MessageSize = MessageSize.Medium,
                UseBufferedSink = true,
                MaxQueueSizeBytes = 10_000_000, // 10 MB
                FlushingAccuracy = 250,
                MessagesPerRequest = 100
            };

            await ExecuteLoadTest(config);
        }

        static async Task RunBurstTest()
        {
            Console.WriteLine("\n>>> Running Burst Test <<<");
            Console.WriteLine("Target: 10000 messages in 10 seconds\n");

            var config = new LoadTestConfig
            {
                TestName = "Burst",
                TargetMessagesPerSecond = 1000,
                DurationSeconds = 10,
                MessageSize = MessageSize.Medium,
                UseBufferedSink = true,
                MaxQueueSizeBytes = 20_000_000, // 20 MB for burst
                FlushingAccuracy = 100,
                MessagesPerRequest = 200
            };

            await ExecuteLoadTest(config);
        }

        static async Task RunSpikeTest()
        {
            Console.WriteLine("\n>>> Running Spike Test <<<");
            Console.WriteLine("Alternating between 100 msgs/sec and 2000 msgs/sec\n");

            var config = new LoadTestConfig
            {
                TestName = "Spike",
                TargetMessagesPerSecond = 100,
                DurationSeconds = 300,
                MessageSize = MessageSize.Medium,
                UseBufferedSink = true,
                MaxQueueSizeBytes = 15_000_000,
                FlushingAccuracy = 200,
                MessagesPerRequest = 150,
                SpikePattern = true,
                SpikeIntervalSeconds = 30,
                SpikeMultiplier = 20
            };

            await ExecuteLoadTest(config);
        }

        static async Task RunStressTest()
        {
            Console.WriteLine("\n>>> Running Stress Test <<<");
            Console.WriteLine("Gradually increasing load from 100 to 5000 msgs/sec\n");

            var config = new LoadTestConfig
            {
                TestName = "Stress",
                TargetMessagesPerSecond = 100,
                DurationSeconds = 300,
                MessageSize = MessageSize.Large,
                UseBufferedSink = true,
                MaxQueueSizeBytes = 50_000_000, // 50 MB
                FlushingAccuracy = 100,
                MessagesPerRequest = 200,
                GradualIncrease = true,
                MaxMessagesPerSecond = 5000
            };

            await ExecuteLoadTest(config);
        }

        static async Task RunEnduranceTest()
        {
            Console.WriteLine("\n>>> Running Endurance Test <<<");
            Console.WriteLine("Moderate load (500 msgs/sec) for 30 minutes\n");

            var config = new LoadTestConfig
            {
                TestName = "Endurance",
                TargetMessagesPerSecond = 500,
                DurationSeconds = 1800, // 30 minutes
                MessageSize = MessageSize.Medium,
                UseBufferedSink = true,
                MaxQueueSizeBytes = 10_000_000,
                FlushingAccuracy = 250,
                MessagesPerRequest = 100
            };

            await ExecuteLoadTest(config);
        }

        static async Task RunCustomTest()
        {
            Console.WriteLine("\n>>> Custom Load Test <<<");
            
            var config = new LoadTestConfig
            {
                TestName = "Custom"
            };

            Console.Write("Messages per second: ");
            config.TargetMessagesPerSecond = int.Parse(Console.ReadLine() ?? "100");

            Console.Write("Duration (seconds): ");
            config.DurationSeconds = int.Parse(Console.ReadLine() ?? "60");

            Console.Write("Message size (1=Small, 2=Medium, 3=Large, 4=XLarge 1MB): ");
            var sizeChoice = Console.ReadLine();
            config.MessageSize = sizeChoice switch
            {
                "1" => MessageSize.Small,
                "2" => MessageSize.Medium,
                "3" => MessageSize.Large,
                "4" => MessageSize.XLarge,
                _ => MessageSize.Medium
            };

            Console.Write("Use buffered sink? (y/n): ");
            config.UseBufferedSink = Console.ReadLine()?.ToLower() == "y";

            if (config.UseBufferedSink)
            {
                Console.Write("Max queue size (MB): ");
                config.MaxQueueSizeBytes = long.Parse(Console.ReadLine() ?? "10") * 1_000_000;

                Console.Write("Flushing accuracy (ms): ");
                config.FlushingAccuracy = long.Parse(Console.ReadLine() ?? "250");

                Console.Write("Messages per request: ");
                config.MessagesPerRequest = long.Parse(Console.ReadLine() ?? "100");
            }

            Console.WriteLine();
            await ExecuteLoadTest(config);
        }

        static async Task ExecuteLoadTest(LoadTestConfig config)
        {
            var logger = CreateLogger(config);
            var stats = new LoadTestStats();
            var stopwatch = Stopwatch.StartNew();
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            // Start statistics reporting task
            var statsTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(5000, token);
                    PrintStats(stats, stopwatch.Elapsed);
                }
            }, token);

            try
            {
                Console.WriteLine($"Starting test: {config.TestName}");
                Console.WriteLine($"Duration: {config.DurationSeconds}s");
                Console.WriteLine($"Target rate: {config.TargetMessagesPerSecond} msgs/sec");
                Console.WriteLine($"Buffered: {config.UseBufferedSink}");
                if (config.UseBufferedSink)
                {
                    Console.WriteLine($"Buffer size: {config.MaxQueueSizeBytes / 1_000_000} MB");
                    Console.WriteLine($"Flushing accuracy: {config.FlushingAccuracy}ms");
                    Console.WriteLine($"Messages per request: {config.MessagesPerRequest}");
                }
                Console.WriteLine("\nTest running... (Ctrl+C to stop)\n");

                var endTime = DateTime.UtcNow.AddSeconds(config.DurationSeconds);
                var messageCounter = 0;

                while (DateTime.UtcNow < endTime)
                {
                    var currentRate = config.TargetMessagesPerSecond;

                    // Apply spike pattern if enabled
                    if (config.SpikePattern)
                    {
                        var elapsedSeconds = (int)stopwatch.Elapsed.TotalSeconds;
                        if ((elapsedSeconds / config.SpikeIntervalSeconds) % 2 == 1)
                        {
                            currentRate *= config.SpikeMultiplier;
                        }
                    }

                    // Apply gradual increase if enabled
                    if (config.GradualIncrease)
                    {
                        var progress = stopwatch.Elapsed.TotalSeconds / config.DurationSeconds;
                        currentRate = config.TargetMessagesPerSecond + 
                            (int)((config.MaxMessagesPerSecond - config.TargetMessagesPerSecond) * progress);
                    }

                    var delayMs = 1000.0 / currentRate;
                    var startLoop = Stopwatch.GetTimestamp();

                    try
                    {
                        var message = GenerateMessage(config.MessageSize, messageCounter++);
                        logger.Information(message);
                        Interlocked.Increment(ref stats.MessagesSent);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref stats.MessagesFailed);
                        Console.WriteLine($"Error sending message: {ex.Message}");
                    }

                    // Precise timing control
                    var elapsedMs = (Stopwatch.GetTimestamp() - startLoop) * 1000.0 / Stopwatch.Frequency;
                    var remainingDelay = delayMs - elapsedMs;
                    
                    if (remainingDelay > 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(remainingDelay));
                    }
                }

                Console.WriteLine("\nTest duration completed. Flushing remaining messages...");
            }
            finally
            {
                cancellationTokenSource.Cancel();
                
                // Flush and close
                await Log.CloseAndFlushAsync();
                
                stopwatch.Stop();

                Console.WriteLine("\n=== FINAL RESULTS ===");
                PrintStats(stats, stopwatch.Elapsed);
                Console.WriteLine($"\nActual rate: {stats.MessagesSent / stopwatch.Elapsed.TotalSeconds:F2} msgs/sec");
                Console.WriteLine($"Success rate: {(stats.MessagesSent - stats.MessagesFailed) * 100.0 / stats.MessagesSent:F2}%");
            }
        }

        static ILogger CreateLogger(LoadTestConfig config)
        {
            var logConfig = new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning);

            if (config.UseBufferedSink)
            {
                logConfig.WriteTo.BufferedSumoLogic(
                    new Uri(SumoLogicEndpoint),
                    sourceName: $"LoadTest-{config.TestName}",
                    sourceCategory: "LoadTest/Serilog",
                    sourceHost: Environment.MachineName,
                    maxQueueSizeBytes: config.MaxQueueSizeBytes,
                    flushingAccuracy: config.FlushingAccuracy,
                    messagesPerRequest: config.MessagesPerRequest,
                    maxFlushInterval: 10000,
                    retryInterval: 5000);
            }
            else
            {
                logConfig.WriteTo.SumoLogic(
                    new Uri(SumoLogicEndpoint),
                    sourceName: $"LoadTest-{config.TestName}",
                    sourceCategory: "LoadTest/Serilog",
                    sourceHost: Environment.MachineName);
            }

            return logConfig.CreateLogger();
        }

        static string GenerateMessage(MessageSize size, int counter)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var baseMessage = $"[{counter}] Load test message at {timestamp}";

            return size switch
            {
                MessageSize.Small => baseMessage,
                MessageSize.Medium => baseMessage + " | " + new string('X', 200),
                MessageSize.Large => baseMessage + " | " + new string('X', 1000),
                MessageSize.XLarge => baseMessage + " | " + new string('X', 1_000_000), // ~1MB message
                _ => baseMessage
            };
        }

        static void PrintStats(LoadTestStats stats, TimeSpan elapsed)
        {
            Console.WriteLine($"[{elapsed:mm\\:ss}] Sent: {stats.MessagesSent:N0} | Failed: {stats.MessagesFailed:N0} | Rate: {stats.MessagesSent / elapsed.TotalSeconds:F2} msg/s");
        }
    }

    class LoadTestConfig
    {
        public string TestName { get; set; }
        public int TargetMessagesPerSecond { get; set; }
        public int DurationSeconds { get; set; }
        public MessageSize MessageSize { get; set; }
        public bool UseBufferedSink { get; set; }
        public long MaxQueueSizeBytes { get; set; }
        public long FlushingAccuracy { get; set; }
        public long MessagesPerRequest { get; set; }
        public bool SpikePattern { get; set; }
        public int SpikeIntervalSeconds { get; set; } = 30;
        public int SpikeMultiplier { get; set; } = 10;
        public bool GradualIncrease { get; set; }
        public int MaxMessagesPerSecond { get; set; } = 5000;
    }

    class LoadTestStats
    {
        public long MessagesSent;
        public long MessagesFailed;
    }

    enum MessageSize
    {
        Small,
        Medium,
        Large,
        XLarge
    }
}
