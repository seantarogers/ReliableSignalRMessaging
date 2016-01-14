# ReliableSignalrMessaging
======

Overview
------

A reliable SignalR server messaging framework prototype. This solution attempts to address the number of challenges when using SignalR as a server messaging framework.  It adds the following reliability features:

Reliable messaging features
------

 * To ensure Hub to client notifications can be retried, the notifications sent from NServiceBus command handler via HubContext (rather than from within a Signalr Hub).
 * Hub client immediately moves received messages from the ring buffer to a persistent queue before processing. This avoids message loss if a client reconnects whilst messages are sitting in the buffer waiting to be processed.
 * Hub notification throttling on Hub server makes certain no notifications are pushed to a client whilst that client is reconnecting due to an access token refresh.
 * Client message acknowledgments not sent back to the Hub, but to a seperately hosted Web Api.  This avoids lost acknowledgements where the Hub is temporarily unavailable.
 * On-client-connect replay policy that checks for unacknowledged dead letters (when a client connetion is received) and replays them to the newly connected client. Messages are only replayed where they fall within a given date and number of retries and have not received an acknowledgment.
 * Hub client processing is managed on a seperate thread in an Nservicebus command handler. This allows for multiple retries of insertion of data into the back office.
 * Hub Client message acknowledgment sending is managed on a seperate thread in an NServicebus command handler.  This allows for multiple retries of insertion of data into the back office.
 * Future feature - Client side message store using Esent to ensure message Idempotency.


To Run
------
1. Create a SQL Server 2014 database called 'NServiceBus' on a SQL instance called '.\sqlserver2014'
2. Add an NserviceBus license to C:\NServiceBus\License.xml
3. Run the solution and submit the relevant button on the UI.
