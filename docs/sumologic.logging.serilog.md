# SumoLogic.Logging.Serilog

Serilog sink for logging events into SumoLogic.

## Installation

```ps
Install-Package Serilog
Install-Package SumoLogic.Logging.Serilog
```

## Basic usage

There are two Serilog sinks included in this package:

- `BufferedSumoLogicSink`: Uses buffer to collect events, which are being sent in batches (extension method: `BufferedSumoLogic`)
- `SumoLogicSink`: Sends event right away (extension method: `SumoLogic`)

### Using config file

To set up logger using configuration file, additional dependency is required - install `Serilog.Settings.Configuration` as well.

```ps
Install-Package Serilog.Settings.Configuration
```

Add `appsettings.json` (or whatever configuration you need) as follows:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "BufferedSumoLogic",
        "Args": {
          "endpointUrl": "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==",
          "sourceName": "ExampleNameSerilogBufferedSink"
        }
      }
    ]
  }
}

```

Notice property `Name` - either use `BufferedSumoLogic` or `SumoLogic`. Detailed description
of configuration properties are listed in [Configuration section](#configuration).

To load such configuration, use Serilog's `ReadFrom` method like this:

```csharp
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Logger log = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

### Using extension method

It is possible set up logger using extension method as well:

```csharp
// alternatively .WriteTo.SumoLogic(...)
Logger log = new LoggerConfiguration()
    .WriteTo.BufferedSumoLogic(
        new Uri("https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here=="),
        sourceName: "ExampleNameSerilogBufferedSink")
    .CreateLogger();
```

### Setting JSON Formatter

When logging to SumoLogic, you may find useful to log JSON instead of plain text message. It is possible
to configure JSON formatter (`Serilog.Formatting.Compact.CompactJsonFormatter`) following way.

Install package containing JSON formatter:

```ps
Install-Package Serilog.Formatting.Compact
```

Then use extension method to assign new formatter to `formatter` argument:

```csharp
new LoggerConfiguration().
    .WriteTo.BufferedSumoLogic(
        new Uri("https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here=="),
        formatter: new CompactJsonFormatter())
    .CreateLogger();
```

Alternatively, you can configure formatter in config file (see [Configuration section](#Config-file)):

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "SumoLogic",
        "Args": {
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
          "endpointUrl": "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==",
        }
      }
    ]
  }
}
```

## Configuration options

Either use as sink arguments in configuration or as name arguments of extension methods.

| Argument (B/*)            | Description                                                                           | Default value         |
|---------------------------|---------------------------------------------------------------------------------------|----------------------:|
| endpointUrl               | SumoLogic endpoint URL, __mandatory__                                                 | `null`                |
| outputTemplate            | A message template describing the format used to write to the sink                    | _see below_           |
| sourceName                | The name used for messages sent to SumoLogic server                                   | `null`                |
| sourceCategory            | The source category for messages sent to SumoLogic server                             | `null`                |
| sourceHost                | The source host for messages sent to SumoLogic Server                                 | `null`                |
| clientName                | The client name value that is included in each request (used for telemetry)           | `null`                |
| connectionTimeout         | The connection timeout, in milliseconds                                               | 60 000                |
| retryInterval (B)         | The send message retry interval, in milliseconds                                      | 10 000                |
| maxFlushInterval (B)      | The maximum interval between flushes, in milliseconds                                 | 10 000                |
| flushingAccuracy (B)      | How often the messages queue is checked for messages to send, in milliseconds         | 250                   |
| messagesPerRequest (B)    | How many messages need to be in the queue before flushing                             | 100                   |
| maxQueueSizeBytes (B)     | The messages queue capacity, in bytes                                                 | 1 000 000             |
| httpMessageHandler        | Override HTTP message handler which manages requests to SumoLogic                     | `null`                |
| formatter                 | Controls the rendering of log events into text, for example to log JSON               | `null`                |
| levelSwitch               | A switch allowing the pass-through minimum level to be changed at runtime             | `null`                |
| restrictedToMinimumLevel  | The minimum level for events passed through the sink, ignored if `levelSwitch` is set | `LevelAlias.Minimum`  |

_arguments marked with "(B)" are available only to buffered sink (`SumoLogicSink`)_

**Notes**:

- default `outputTemplate` = `"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message} {Exception}"`
- provide `HttpMessageHandler httpMessageHandler` to adjust HTTP request sent SumoLogic
- when `LoggingLevelSwitch levelSwitch` is set, `LogEventLevel restrictedToMinimumLevel` argument is ignored
- when setting text formatter (`ITextFormatter formatter`), always use fully qualified type name (if set, `string outputTemplate` is ignored)
