# ReliableSignalRMessaging

## Overview

A reliable SignalR server messaging framework prototype. This solution attempts to address the number of challenges when using SignalR as a server messaging framework. It adds the following reliability features:

## Reliable messaging features

 * Robust management of Client Hub connection including reconnection retry, managed disposal and guaranteed single connection at any point in time (which avoids duplicate message delivery).
 * Lightweight client side message store using Esent (Extensible Storage Engine) to ensure message Idempotency.
 * To ensure Hub to Hub Client notifications can be retried, the notifications are sent from NServiceBus command handler via HubContext (rather than from within a Signalr Hub).
 * Hub Client immediately moves received messages from the volatile buffer to a persistent queue before processing. This avoids message loss if a Hub Client reconnects whilst messages are sitting in the buffer waiting to be processed.
 * Hub notification throttling on Hub server makes certain no notifications are pushed to a client whilst that Hub Client is reconnecting due to an access token refresh. Again this avoids message loss.
 * On-client-connect replay policy that checks for un-acknowledged dead letters (when a client connection is received) and replays them to the newly connected client. Messages are only replayed where they meet the replay criteria of a given date and number of retries and have not received an acknowledgment.
 * Hub Client message acknowledgments not sent back to the Hub. They are posted to a separately hosted Web Api.  This avoids lost acknowledgements where the Hub is temporarily unavailable.  This also enables the replay policy to work - if we resent acknowledgments over the hub connection, and we replayed messages on-connect, the replay would happen before the acknowledgment was received.  Leading to potential duplicates (if you did not have a message store).
 * Hub client processing is managed on a separate thread in an Nservicebus command handler. This allows for multiple retries of insertion of data into the Back Office.
 * Hub Client message acknowledgment sending is also managed on a separate thread in an NServicebus command handler.  This allows for multiple retries of sending of acknowledgments. Reducing the risk of duplicate messages.
 

## Other features

* Secure SignalR connections using OAuth2 with JWT.
* Hosting of SignalR on Katana.
* Hosting of OAuth2 Token provider on Katana.
* Workflow managed via NServiceBus Saga.
* SignalR Hub Log4Net tracing.
* Full auditing of all messages sent and received

## To run solution

1. Create a SQL Server 2014 database called 'NServiceBus' on a SQL instance called '.\sqlserver2014'
2. Add an NserviceBus license to C:\NServiceBus\License.xml
3. Set the following projects to run in the solution: Audit, DocumentDownloader, Hub, HubSubscriber, IdentityProvider, OnlineBackOffice, Saga and Ui.
3. On the UI home page select the type of scenario and submit. 


## Architecture

![Image of Architecture](https://raw.githubusercontent.com/seantarogers/ReliableSignalRMessaging/master/ReliableSignalRMessaging.png)

