Description:

	SumoLogic provides an interface between NLog and remote sevice Logging. You can logging its application as the usual way.
	Once you have downloaded the nuget package, you can watch the example project for configuration details.

Minimum requirements:
	- .Net 4.0

Targets:
	- SumoLogicTarget: It instantly pushes the log message to SumoLogic.
	- BufferedSumoLogicTarget: It makes a log messages queue. It pushes the queue if the maximum quantity of messages was reached or if the maximum flush interval has passed.

Configuration:
	- The configuration is in NLog.config. There are two target, BufferedSumoLogicTarget and SumoLogicTarget.
	- We recommend to use the BufferedSumoLogicTarget because SumoLogicTarget might make the application runs slower.
	- The output url is which you get from SumoLogic http collector.
	
Properties:

	SumoLogicTarget: 

		- Url: The http collector URL from SumoLogic.
		- SourceName: The named used for messages sent to SumoLogic.
		- SourceCategory: The category used for messages sent to SumoLogic.
		- SourceHost: The host used for messages sent to SumoLogic.
		- ConnectionTimeout: The connection timeout in milliseconds.
	

	BufferedSumoLogicTarget: 
		- Url: The http collector URL from SumoLogic.				 
		- SourceName: The named used for messages sent to SumoLogic.
		- SourceCategory: The category used for messages sent to SumoLogic.
		- SourceHost: The host used for messages sent to SumoLogic.
		- ConnectionTimeout: The connection timeout in milliseconds.
		- FlushingAccuracy: How often the messages queue is checked for messages to send, in milliseconds.
		- MaxFlushInterval: The maximum interval between flushes, in milliseconds.	
		- MessagePerRequest: How many messages need to be in the queue before flushing.
		- MaxQueueSizeBytes: The messages queue capacity, in bytes. Messages are dropped When the queue capacity is exceeded.  
      


	
