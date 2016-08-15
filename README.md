# Sumo-net-loggers

Several appenders for .NET developers to use that send logs straight to SumoLogic.

# Prerequisites
* .NET 4.0 or later
* A SumoLogic Account, trial can be started [here](https://www.sumologic.com/)

# Appenders

There are two appenders which are contained in this project.
* NLog
* Log4net

Both appenders have two implementations a Buffering and non Buffering version.
The non Buffering implementation will send each log to SumoLogic, the Buffering will queue them up and send it in batch.

# Nuget Installation

## NLog
To install the NLog appender, follow the following steps
```
PM> Install-Package SumoLogic.Logging.NLog
```

### Example Appender Configuration
```
<?xml version="1.0" encoding="utf-8" ?>
<nlog autoReload="true" internalLogToConsole="true">
	<extensions>
		<add assembly="SumoLogic.Logging.NLog"/>
	</extensions>
	<targets>
		<target name="sumoLogic" type="SumoLogicTarget"	layout="${date:format=yyyy-MM-dd HH\:mm\:ss.fff ${LEVEL}, ${message}">
			<Url>https://collectors.us2.sumologic.com/receiver/v1/http/ZaVnC4dhaV2dpl93h4mEkdCBwxHuX5fI1Yh_75Lhk8GtiMxsATMRTuebaZTDknk5dlFvjvYI7ZvraaHaA2NPq-O4v9bKZSTaMEZ_qHYxQ_ICBlWAonxtGA==</Url>
			<ConnectionTimeout>30000</ConnectionTimeout>
			<SourceName>ExampleNameNLogTarget</SourceName>
			<UseConsoleLog>true</UseConsoleLog>
		</target>
		<target name="bufferedSumoLogic" type="BufferedSumoLogicTarget" layout="${date:format=yyyy-MM-dd HH\:mm\:ss.fff ${LEVEL}, ${message}">
			<Url>https://collectors.us2.sumologic.com/receiver/v1/http/ZaVnC4dhaV2dpl93h4mEkdCBwxHuX5fI1Yh_75Lhk8GtiMxsATMRTuebaZTDknk5dlFvjvYI7ZvraaHaA2NPq-O4v9bKZSTaMEZ_qHYxQ_ICBlWAonxtGA==</Url>
			<SourceName>ExampleNameNLogBufferedTarget</SourceName>
			<ConnectionTimeout>30000</ConnectionTimeout>
			<RetryInterval>5000</RetryInterval>
			<MessagesPerRequest>10</MessagesPerRequest>
			<MaxFlushInterval>10000</MaxFlushInterval>
			<FlushingAccuracy>250</FlushingAccuracy>
			<MaxQueueSizeBytes>500000</MaxQueueSizeBytes>
			<UseConsoleLog>true</UseConsoleLog>
		</target>
	</targets>
	<rules>
		<logger name="*" minLevel="Debug" writeTo="sumoLogic"/>
		<logger name="*" minLevel="Debug" writeTo="bufferedSumoLogic"/>
	</rules>

</nlog>
```

### Example Code
```csharp
public static class Program
{
  /// <summary>
  /// An example application that logs.
  /// </summary>
  public static void Main()
  {
    Logger logger = LogManager.GetCurrentClassLogger();
    logger.Debug("Log message");
    Console.Read();
  }
}
```

### Internal Logging

The Sumo Logic NLog appender can log internal status information and error messages for diagnostic purposes if needed. The
simplest way to enable this is to set `UseConsoleLog = true` through the configuration XML. Internal logging will then be printed to
the console.

If an alternative internal logging method is required, you can optionally specify a custom logger. Implement the interface 
`SumoLogic.Logging.Common.Log.ILog` and reconfigure targets at runtime per below:

```csharp
static void ReconfigureSumoTargets()
{
    foreach (var target in LogManager.Configuration.AllTargets)
    {
        if (!(target is SumoLogicTarget))
            continue;

        var originalTarget = target as SumoLogicTarget;

        var customTargetLogger = new ILogImpl();  // custom implementation of ILog goes here

        var newTarget = new SumoLogicTarget(customTargetLogger, null)
        {
            AppendException = originalTarget.AppendException,
            ConnectionTimeout = originalTarget.ConnectionTimeout,
            Layout = originalTarget.Layout,
            Name = originalTarget.Name,
            SourceName = originalTarget.SourceName,
            Url = originalTarget.Url,
            UseConsoleLog = false
        };

        if (originalTarget.Name != null)
        {
            LogManager.Configuration.RemoveTarget(originalTarget.Name);
            LogManager.Configuration.AddTarget(newTarget.Name, newTarget);
        }

        foreach (var rule in LogManager.Configuration.LoggingRules)
        {
            if (rule.Targets.Remove(originalTarget))
            {
                rule.Targets.Add(newTarget);
            }
        }
    }

    LogManager.ReconfigExistingLoggers();
}
```

## Log4net
To install the Log4Net appender, follow the following steps:
```
PM> Install-Package SumoLogic.Logging.Log4Net
```

### Example Appender Configuration
```xml
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
  </configSections>
  <log4net debug="true">
    <appender name="SumoLogicAppender" type="SumoLogic.Logging.Log4Net.SumoLogicAppender, SumoLogic.Logging.Log4Net">
	  <layout type="log4net.Layout.PatternLayout">
	    <conversionPattern value="%date [%thread] %-5level %logger - %message%newline"/>
	  </layout>
	  <Url value="https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==" />
	  <ConnectionTimeout value="30000" /> <!-- in milliseconds -->
	  <SourceName value="ExampleNameLog4NetAppender" />
	  <UseConsoleLog value="true" />
    </appender>
	<appender name="BufferedSumoLogicAppender" type="SumoLogic.Logging.Log4Net.BufferedSumoLogicAppender, SumoLogic.Logging.Log4Net">
	  <layout type="log4net.Layout.PatternLayout">
	    <conversionPattern value="%date [%thread] %-5level %logger - %message%newline"/>
      </layout>
	  <Url value="https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==" />
	  <SourceName value="ExampleNameLog4NetBufferedAppender" />
      <ConnectionTimeout value="30000" />
      <RetryInterval value="5000" />
      <MessagesPerRequest value="10" />
      <MaxFlushInterval value="10000" />
      <FlushingAccuracy value="250" />
      <MaxQueueSizeBytes value="500000" />
	  <UseConsoleLog value="true" />
	</appender>
	<root>
	  <priority value="ALL"/>
	  <level value="ALL"/>
	  <appender-ref ref="SumoLogicAppender"/>
	  <appender-ref ref="BufferedSumoLogicAppender"/>
	</root>
	<logger name="SumoLogic.Logging.Log4Net.Example.Program">
	  <level value="ALL"/>
	</logger>
  </log4net>

  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
  </startup>

</configuration>

```

### Example Code
```csharp
public static class Program
{
  /// <summary>
  /// The log4net log.
  /// </summary>
  private static ILog log4netLog = LogManager.GetLogger(typeof(Program));

  /// <summary>
  /// An example application that logs.
  /// </summary>
  public static void Main()
  {
    Console.WriteLine("Hello world!");
    log4netLog.Info("Hello world!");
    Console.ReadKey();
  }
}
```


# License
Apache 2.0

# Info
Please see the [wiki](https://github.com/mcplusa/sumologic-net-appenders/wiki)

# Bugs
Please create an [issue](https://github.com/mcplusa/sumologic-net-appenders/issues)

