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
  - Message size (Small/Medium/Large/XLarge/JSON from file)
  - Buffered vs Unbuffered
  - Buffer configuration
  - Custom JSON file path (when using JSON from file option)

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
- **Large:** ~57000 bytes (includes 57000 char payload)
- **XLarge:** ~1 MB (1,048,576 bytes of random characters)
- **JSON from file:** Variable size (reads from a JSON file and injects counter for uniqueness)

## Monitoring for Issues

Watch for these warnings in console output:
- `"Evicted X messages from buffer"` - Buffer overflow, increase `maxQueueSizeBytes`
- `"Sink not initialized"` - Configuration error
- High failure rate - Network or endpoint issues

## Endpoint Configuration

The test is pre-configured with the endpoint:
```
https://collectors.sumologic.com/receiver/v1/http/YOUR_ENDPOINT_HERE
```

To use a different endpoint, modify the `SumoLogicEndpoint` constant in `Program.cs`.

## Using Custom JSON Messages

The load test supports sending custom JSON payloads from files:

1. Create a JSON file with your message structure (e.g., `sample-message.json`)
2. Select option 6 (Custom Test) from the main menu
3. Choose option 5 (JSON from file) for message size
4. Provide the path to your JSON file (or press Enter for default `sample-message.json`)

The tool will:
- Read your JSON file once at startup
- Inject a unique counter into each message for tracking: `"counter":{counter},"timestamp":`
- Send the message repeatedly according to your test parameters

Example JSON structure (`sample-message.json` included):
```json
{
  "timestamp": "2024-01-15T10:30:45.123Z",
  "orderId": "ORD-20240115-4829",
  "customerId": "CUST-A1B2C3D4",
  "payment": {
    "amount": 149.99,
    "currency": "USD",
    "method": "credit_card"
  }
}
```

After processing, each message will have a counter injected:
```json
{
  "counter": 1,
  "timestamp": "2024-01-15T10:30:45.123Z",
  ...
}
```

This allows you to:
- Test with realistic message structures from your production system
- Track individual messages in Sumo Logic using the counter field
- Measure throughput with actual payload sizes

## Understanding Results

### Good Performance Indicators:
- Actual rate matches target rate
- Zero or minimal failed messages
- No "Evicted messages" warnings

### Performance Issues:
- Actual rate < target rate (backpressure)
- High failure rate (network/endpoint issues)
- Many evicted messages (buffer too small)

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
