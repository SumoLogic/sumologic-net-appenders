# SumoLogic.Logging.AspNetCore

## Description

Sumo Logic provides an [ASP.NET Core Logging Provider](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2) for integrating your ASP.NET Core project with Sumo Logic logging system.
Once you have downloaded the NuGet package, you can watch the example project for configuration details.

## Minimum requirements

- ASP.NET Core 2.0

## Provider

After using namespace `SumoLogic.Logging.AspNetCore`, a logging provider named "SumoLogic" is imported. You can also use [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.2) (e.g. DI) to register the provider into your project. Please refer to the example project for details

## Configuration

- Depends on the registering approach, you can either specify the settings in code or in config file (like in `appsettings.json`)
- The provider support both buffered or instance mode. In instance mode, message lines are instantly pushed to Sumo Logic. In buffered mode (by default), message will be stored in a messages queue. It pushes the queue if the maximum quantity of messages was reached or if the maximum flush interval has passed.
- We recommend to use the buffered mode in production environment because of the performance benefit.
- The output url is which you get from SumoLogic http collector.

## Properties

| Argument                  | Description                                                                           | Default value         |
|---------------------------|---------------------------------------------------------------------------------------|----------------------:|
| Uri                       | The http collector URL from SumoLogic.                                                | __mandatory__         |
| SourceName                | The named used for messages sent to SumoLogic.                                        | `asp.net-core-logger` |
| SourceCategory            | The category used for messages sent to SumoLogic.                                     | `null`                |
| SourceHost                | The host used for messages sent to SumoLogic.                                         | DNS host name         |
| ConnectionTimeout         | The connection timeout in milliseconds.                                               | `60000`               |
| IsBuffered                | `true` for buffered mode, `false` for instance mode                                   | `true`                |
| RetryInterval             | Retry after specific of time when message failed to deliver                           | `10s`                 |
| MaxFlushInterval          | The maximum interval between flushes                                                  | `10s`                 |
| FlushingAccuracy          | How often the messages queue is checked for messages to send.                         | `250ms`               |
| MessagePerRequest         | How many messages need to be in the queue before flushing.                            | `100`                 |
| MaxQueueSizeBytes         | The messages queue capacity in bytes. Messages will be dropped when it is exceeded.   | `1000000`             |
