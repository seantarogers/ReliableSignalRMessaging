# ReliableSignalrMessaging
======

A prototype reliable SignalR server messaging framework. This solution attempts to address the number of challenges when using SignalR as a server messaging framework.  It adds the following reliability features:

 * To ensure Hub to client notifications can be retried the notifications sent from NServiceBus command handler via HubContext.
 * Hub client immediately moves received messages from ring buffer to persistent queue before processing avoiding message loss if a client reconnects whilst messages are in the buffer waiting to be processed.
 * Hub notification throttling on Hub server makes certain no notifications are pushed whilst a client reconnect is happening
 * Client message acknowledgments not sent back to the Hub, but to a seperate hosted web api.  This avoids lost acknowledgements where the Hub is temporarily unavailable
 * On-client-connect replay policy that checks dead letters and replays them to the newly connected client where the messages fall within a given date and number of retries and have not received an acknowledgment.
 * Hub client processing is managed on a seperate thread (in an Nservicebus command handler) than the message was received allowing for multiple retries of insertion of data into the back office.
 * Client side message store using Esent. (tbc)


To Run
------
1. Create a SQL Server 2014 database called 'NServiceBus' on a SQL instance called '.\sqlserver2014'
2. Add an NserviceBus license to C:\NServiceBus\License.xml
3. Run the solution and submit the relevant button on the UI.
