# SumoLogic.Logging.AspNetCore

SumoLogic provider for [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging).

## Installation

```ps
Install-Package SumoLogic.Logging.AspNetCore
```

## Configuration

The configuration is done in code via LoggerOptions class. 
There are two modes that control how logs are pushed to sumologic: Buffered and instant.
It is recommended to use the Buffered which is a default, because Instant mode might make the application run slower.

### Configuration options

All options below are properties of LoggerOptions class.

| Argument (B/*)            | Description                                                                           | Default value         |
|---------------------------|---------------------------------------------------------------------------------------|----------------------:|
| Uri                       | SumoLogic endpoint URL, __mandatory__                                                 | `null`                |
| IsBuffered                | Specifies weather Logger should accumulate logs or send them at once.                 | true                   |
| MessageFormatterFunc      | Controls the rendering of log events into text, for example to log JSON.              | See below              |
| SourceName                | The name of the source used for messages sent to SumoLogic server                     | `asp.net-core-logger`|
| SourceCategory            | The source category for messages sent to SumoLogic server                             | `null`                |
| SourceHost                | The source host for messages sent to SumoLogic Server                                 | System.Net.Dns.GetHostName();|
| ConnectionTimeout         | The connection timeout                                                                | 60 seconds            |
| RetryInterval (B)         | The send message retry interval                                                       | 10 seconds            |
| MaxFlushInterval (B)      | The maximum interval between flushes                                                  | 10 seconds            |
| FlushingAccuracy (B)      | How often the messages queue is checked for messages to send                          | 250 milliseconds      |
| MessagesPerRequest (B)    | How many messages need to be in the queue before flushing                             | 100                   |
| MaxQueueSizeBytes (B)     | The messages queue capacity, in bytes                                                 | 1 000 000             |
| HttpMessageHandler        | Override HTTP message handler which manages requests to SumoLogic                     | `null`               |
| MinLogLevel               | Min accpated Log Level.                                                               | LogLevel.Information |
| EnableScopes              | Enable Logger Scopes support                                                          | true                 |

_arguments marked with "(B)" are available only to buffered sink (`IsBuffered = true`)_


### Logger Registration

To register logger, call AddSumoLogic extenstion method with during startup of Asp.Net Core application or Azure Function App:

#### Asp.Net Core
```csharp

 public void Configure(IApplicationBuilder app, 
                       IHostingEnvironment env,
                       ILoggerFactory loggerFactory)
{
  loggerFactory.AddConsole();
  
  loggerFactory.AddSumoLogic(
    new LoggerOptions{
      Uri = "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here=="
    });
  }

  // more removed
}
```

#### AzureFunctions
```csharp

 public class LoggingStartup : IWebJobsStartup
 {
    public void Configure(IWebJobsBuilder builder)
    {
      builder.Services.AddSumoLogic(
        new LoggerOptions{
          Uri = "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here=="
       });
     }
  }
```

## MessageFormatterFunc


### Setting JSON Formatter

When logging to SumoLogic, you may find useful to log JSON or XML instead of plain text message or to change the message. It is possible
to configure JSON formatter by providing formatter Func.

The default function is:
```csharp
public Func<string, Exception, string, LogLevel, IDictionary<string, object>, string> MessageFormatterFunc { get; set; }
            = (message, ex, category, level, scopedProperties) => 
$"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] {message} {ex}";
```

You can change it by setting MessageFormatterFunc in LoggerOptions like this

```csharp
var loggerOptions = new LoggerOptions
{
    Uri = "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==",
    MessageFormatterFunc = (message, ex, category, logLevel, properties) =>
    {
        var messageProperties = new Dictionary<string, string>();
        messageProperties["date"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"); //use nlog format for datetime with timezone offset

        foreach (var o in properties.Where(x => x.Value != null && x.Key != "date"))
        {
            messageProperties[o.Key] = o.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            messageProperties["message"] = message;
        }

        messageProperties["level"] = logLevel.ToString();

        if (ex != null)
        {
            messageProperties["exception"] = ex.ToString();
        }

        if (category != null)
        {
            messageProperties["category"] = category;
        }

        return JsonConvert.SerializeObject(messageProperties);
    }
};
```

Please note, in the example above, we put the date property first, because sumo logic considers time of the log message as a first date time it finds in the log message.