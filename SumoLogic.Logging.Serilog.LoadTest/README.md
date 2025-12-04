# Sumo Logic Serilog Load Test

This project provides comprehensive load testing capabilities for the SumoLogic.Logging.Serilog appender.

## Quick Start

```bash
cd SumoLogic.Logging.Serilog.LoadTest
dotnet run
```

## Pre-configured Test Scenarios

### 1. Sustained Load Test
- **Target:** 1000 messages/second
- **Duration:** 5 minutes
- **Purpose:** Test steady-state performance
- **Buffer:** 10 MB

### 2. Burst Test
- **Target:** 10,000 messages in 10 seconds (1000 msg/s)
- **Duration:** 10 seconds
- **Purpose:** Test short-term burst handling
- **Buffer:** 20 MB

### 3. Spike Test
- **Pattern:** Alternates between 100 msg/s and 2000 msg/s
- **Duration:** 5 minutes
- **Purpose:** Test handling of traffic spikes
- **Buffer:** 15 MB

### 4. Stress Test
- **Pattern:** Gradually increases from 100 to 5000 msg/s
- **Duration:** 5 minutes
- **Purpose:** Find breaking points
- **Buffer:** 50 MB

### 5. Endurance Test
- **Target:** 500 messages/second
- **Duration:** 30 minutes
- **Purpose:** Test long-term stability
- **Buffer:** 10 MB

### 6. Custom Test
- Configure your own parameters:
  - Messages per second
  - Duration
  - Message size (Small/Medium/Large)
  - Buffered vs Unbuffered
  - Buffer configuration

## Configuration Parameters

When using buffered sink, you can configure:

- **Max Queue Size:** Buffer capacity in bytes (prevents memory overflow)
- **Flushing Accuracy:** How often to check queue (milliseconds)
- **Messages Per Request:** Batch size for HTTP requests

## Metrics Reported

During the test, you'll see real-time statistics every 5 seconds:
- **Messages Sent:** Total messages successfully queued
- **Failed:** Messages that encountered errors
- **Rate:** Actual messages per second achieved

## Example Output

```
=== Sumo Logic Serilog Load Test ===
Endpoint: https://long-endpoint1-events.sumologic.net/re...

Select Load Test Scenario:
1. Sustained Load Test (1000 msgs/sec for 5 minutes)
2. Burst Test (10000 msgs in 10 seconds)
...

>>> Running Sustained Load Test <<<
Target: 1000 messages/second for 5 minutes

Starting test: Sustained Load
Duration: 300s
Target rate: 1000 msgs/sec
Buffered: True
Buffer size: 10 MB
Flushing accuracy: 250ms
Messages per request: 100

Test running... (Ctrl+C to stop)

[00:05] Sent: 5,023 | Failed: 0 | Rate: 1004.60 msg/s
[00:10] Sent: 10,011 | Failed: 0 | Rate: 1001.10 msg/s
...

Test duration completed. Flushing remaining messages...

=== FINAL RESULTS ===
[05:00] Sent: 300,234 | Failed: 0 | Rate: 1000.78 msg/s

Actual rate: 1000.78 msgs/sec
Success rate: 100.00%
```

## Message Sizes

- **Small:** ~100 bytes (timestamp + counter)
- **Medium:** ~300 bytes (includes 200 char payload)
- **Large:** ~1100 bytes (includes 1000 char payload)

## Monitoring for Issues

Watch for these warnings in console output:
- `"Evicted X messages from buffer"` - Buffer overflow, increase `maxQueueSizeBytes`
- `"Sink not initialized"` - Configuration error
- High failure rate - Network or endpoint issues

## Tips

1. **Start small:** Begin with Burst Test (10 seconds) to verify connectivity
2. **Monitor Sumo Logic:** Check that logs are appearing in your Sumo Logic account
3. **Adjust buffer:** If you see eviction warnings, increase `maxQueueSizeBytes`
4. **Network matters:** Results will vary based on network latency to Sumo Logic

## Endpoint Configuration

The test is pre-configured with the endpoint:
```
https://long-endpoint1-events.sumologic.net/receiver/v1/http/ZaVnC4dhaV...
```

To use a different endpoint, modify the `SumoLogicEndpoint` constant in `Program.cs`.

## Understanding Results

### Good Performance Indicators:
- Actual rate matches target rate
- Zero or minimal failed messages
- No "Evicted messages" warnings
- Logs appear in Sumo Logic with minimal delay

### Performance Issues:
- Actual rate < target rate (backpressure)
- High failure rate (network/endpoint issues)
- Many evicted messages (buffer too small)
- Logs delayed or missing in Sumo Logic

## Advanced Usage

### Running from Command Line

```bash
# Build
dotnet build

# Run
dotnet run

# Run with release optimizations
dotnet run -c Release
```

