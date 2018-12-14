Description:

	SumoLogic provides an interface between Serilog and remote sevice Logging. You can logging its application as the usual way.
	Once you have downloaded the nuget package, you can watch the example project for configuration details.

Minimum requirements:
	- .Net 4.5

Sinks:
	- SumoLogicSink: It instantly pushes the log message to SumoLogic.
	- BufferedSumoLogicSink: It makes a log messages queue. It pushes the queue if the maximum quantity of messages was reached or if the maximum flush interval has passed.

Configuration:
	- The configuration is done in code or via appsettings.json. There are two sinks, BufferedSumoLogicSink and SumoLogicSink.
	- We recommend to use the BufferedSumoLogicSink because SumoLogicSink might make the application runs slower.
	- The output url is which you get from SumoLogic http collector.
	
Arguments:

	SumoLogicSink:

		- endpointUrl: The http collector URL from SumoLogic.
		- sourceName: The named used for messages sent to SumoLogic.
		- sourceCategory: The category used for messags sent to SumoLogic.
		- sourceHost: The host used for messages sent to SumoLogic.
		- clientName: The client name value that is included in each request (used for telemetry).
		- connectionTimeout: The connection timeout in milliseconds.

	BufferedSumoLogicSink:
	
		- endpointUrl: The http collector URL from SumoLogic.
		- sourceName: The named used for messages sent to SumoLogic.
		- sourceCategory: The category used for messags sent to SumoLogic.
		- sourceHost: The host used for messages sent to SumoLogic.
		- clientName: The client name value that is included in each request (used for telemetry).
		- connectionTimeout: The connection timeout in milliseconds.
		- flushingAccuracy: How often the messages queue is checked for messages to send, in milliseconds.
		- maxFlushInterval: The maximum interval between flushes, in milliseconds.	
		- messagesPerRequest: How many messages need to be in the queue before flushing.
		- maxQueueSizeBytes: The messages queue capacity, in bytes. Messages are dropped When the queue capacity is exceeded.  
