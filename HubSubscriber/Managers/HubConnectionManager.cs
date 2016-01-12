namespace HubSubscriber.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;

    using Contracts;

    using Logger;

    using Messages.Commands;

    using Services;

    using Microsoft.AspNet.SignalR.Client;
    using Microsoft.AspNet.SignalR.Client.Transports;

    using NServiceBus;

    public class HubConnectionManager : IHubConnectionManager
    {
        private const string HubName = "backOfficeHub";

        private IHubProxy<IBackOfficeHub, IBackOfficeHubClient> hubProxy;
        
        private HubConnection hubConnection;

        private readonly IAccessTokenService accessTokenService;

        private readonly IBus bus;

        private readonly IMessagingLogger messagingLogger;

        private DateTime accessTokenExpiryDate;

        private readonly object startLock = new object();

        private readonly object refreshTokenLock = new object();


        public HubConnectionManager(IAccessTokenService accessTokenService, IBus bus, IMessagingLogger messagingLogger)
        {
            this.accessTokenService = accessTokenService;
            this.bus = bus;
            this.messagingLogger = messagingLogger;
        }

        public bool Start()
        {
            lock (startLock)
            {
                try
                {
                    return StartConnectionToHub();
                }
                catch (AggregateException e)
                {
                    messagingLogger.ErrorFormat(this, e.ToString());
                }
                catch (Exception e)
                {
                    messagingLogger.ErrorFormat(this, e.ToString());
                }

                return false;
            }
        }

        public void Stop()
        {
            if (hubProxy != null)
            {
                Console.WriteLine("Disposing of hub proxy");
                hubProxy.Dispose();
            }

            if (hubConnection != null)
            {
                Console.WriteLine("Stopping hub connection");
                //fyi issues an abort message to the server
                hubConnection.Stop();
            }
        }


        public bool AccessTokenNeedsRefresh()
        {
            lock (refreshTokenLock)
            {
                return accessTokenExpiryDate <= DateTime.UtcNow.AddMinutes(5);
            }
        }

        private bool StartConnectionToHub()
        {
            if (HubConnectionIsAlreadyConnected())
            {
                return false;
            }
            
            //add retry policy to get access token
            var tokenContent = accessTokenService.GetToken();
            var accessToken = accessTokenService.ExtractAccessToken(tokenContent);
            
            accessTokenExpiryDate = accessTokenService.ExtractAccessTokenExpiryDate(tokenContent);

            var queryString = new Dictionary<string, string> { { "bearer_token", HttpUtility.UrlEncode(accessToken) } };

            var hubUrl = "http://localhost:8093";

            hubConnection = new HubConnection(hubUrl, queryString)
                                {
                                    TraceLevel = TraceLevels.All,
                                    TraceWriter = new Log4NetTextWriter(new MessagingLogger()),
                                    DeadlockErrorTimeout = TimeSpan.FromMinutes(5)
                                };

            RegisterHubConnectionEvents();
            
            hubProxy = hubConnection.CreateHubProxy<IBackOfficeHub, IBackOfficeHubClient>(HubName);

            //subscribe before connection is started so that you can make your replay all messages call 
            // inside the connection event
            SetUpHubConnectionSubscriptions();

            hubConnection.Start(new ServerSentEventsTransport()).Wait();

            return true;
        }

        private void SetUpHubConnectionSubscriptions()
        {
            hubProxy.SubscribeOn<InsertDocumentIntoRemoteBackOfficeCommand>(
                hub => hub.InsertDocument,
                SendInsertDocumentCommand);
        }

        private bool HubConnectionIsAlreadyConnected()
        {
            return hubConnection != null && hubConnection.State == ConnectionState.Connected;
        }

        private void SendInsertDocumentCommand(InsertDocumentIntoRemoteBackOfficeCommand command)
        {
            //take message out of buffer immediately and persist to msmq
            bus.Send(command);
        }

        private void RegisterHubConnectionEvents()
        {
            hubConnection.Error += HubConnectionOnError;
            hubConnection.Closed += HubConnectionOnClosed;
            hubConnection.StateChanged += HubConnectionStateChanged;
            hubConnection.Received += HubConnectionReceivedData;
            hubConnection.ConnectionSlow += HubConnectionSlow;
        }

        private void HubConnectionSlow()
        {
            Console.WriteLine("Hub connection has not received a keep alive for 30 seconds");
            hubConnection.Stop();
        }

        private void HubConnectionOnError(Exception exception)
        {
            Console.WriteLine("Hub connection has raised an exception. Details: {0}", exception);
            //the following will trigger an on closed event, then reschedule a restart
            hubConnection.Stop();
        }

        private void HubConnectionOnClosed()
        {
            Console.WriteLine("Scheduling a restart");

            Task.Delay(new TimeSpan(0, 0, 30))
                .ContinueWith(t => Start());
        }

        private static void HubConnectionReceivedData(string data)
        {
            Console.WriteLine("Hub connection has received the following data: {0}", data);
        }

        private static void HubConnectionStateChanged(StateChange stateChange)
        {
            Console.WriteLine(
                "Hub connection's state has changed from oldstate: {0} to new state: {1}",
                stateChange.OldState,
                stateChange.NewState);
        }
    }
}




//private StartResultType Start()
//{
//    asiLogger.DebugFormat(this, HubConnectionConstants.HubConnectionStarted);

//    if (HubConnectionIsAlreadyConnected())
//    {
//        asiLogger.DebugFormat(this, HubConnectionConstants.HubConnectionIsAlreadyConnected);
//        return StartResultType.ThereIsAlreadyAnActiveConnectionDoNotNeedToStartAnother;
//    }

//    asiLogger.DebugFormat(this, HubConnectionConstants.HubConnectionIsNotAlreadyConnected);

//    string token;
//    if (!tokenService.TryGetTokenWithRetries(out token))
//    {
//        asiLogger.ErrorFormat(this, HubConnectionConstants.UnableToGetAToken);
//        RaiseOnRestartEvent();
//        return StartResultType.AnExceptionWasRaisedDuringTheStart;
//    }

//    var authQueryString = authorisationQueryStringFactory.Create(token);

//    accessTokenExpiryDate = tokenContentService.ExtractExpiryDate(token);
//    asiLogger.DebugFormat(this, HubConnectionConstants.ExtractedTokenExpiryDate);

//    hubConnectionAdapter = hubConnectionAdapterFactory.Create(configurationProvider.HubUrl, authQueryString);
//    RegisterHubConnectionEvents();

//    hubProxy = hubConnectionAdapter.CreateHubProxy<IBackOfficeHub, IBackOfficeHubClient>(HubName);

//    hubConnectionAdapter.Start(new ServerSentEventsTransport()).Wait();

//    asiLogger.DebugFormat(this, HubConnectionConstants.StartedHubConnection, configurationProvider.HubUrl);

//    return StartResultType.SuccessfullyStartedNewConnection;
//}

//private void RegisterHubConnectionEvents()
//{
//    hubConnectionAdapter.StateChanged(HubConnectionOnStateChanged);
//    hubConnectionAdapter.Error(HubConnectionOnError);
//    hubConnectionAdapter.Closed(HubConnectionOnClosed);
//    hubConnectionAdapter.Received(HubConnectionOnReceivedData);
//    hubConnectionAdapter.ConnectionSlow(HubConnectionOnSlow);
//}

////This handler is called when an existing hub connection errors i.e for a lost connection.
////it is not called when connection.start() throws an error upon calling
//private void HubConnectionOnError(Exception exception)
//{
//    asiLogger.ErrorFormat(this, HubConnectionConstants.HubConnectionEventError, exception);
//    StopConnectionToHub();
//}

////This handler called when a hubconnection.stop() is called - either manually, 
//// or when we try and start() one and it cannot connect to the hub
//private void HubConnectionOnClosed()
//{
//    asiLogger.InfoFormat(this, HubConnectionConstants.HubConnectionClosedEvent);
//    RaiseOnRestartEvent();
//}

//private void HubConnectionOnSlow()
//{
//    asiLogger.InfoFormat(this, HubConnectionConstants.ConnectionSlow);
//}

//private void HubConnectionOnReceivedData(string dataDetails)
//{
//    asiLogger.InfoFormat(this, HubConnectionConstants.ReceivedData);
//}

//private void HubConnectionOnStateChanged(StateChange stateChange)
//{
//    asiLogger.InfoFormat(this, HubConnectionConstants.ChangingState, stateChange.OldState,
//        stateChange.NewState);
//}

//private void RaiseOnRestartEvent()
//{
//    if (OnRestart == null)
//    {
//        throw new ApplicationException(HubConnectionConstants.OnRestartIsNull);
//    }

//    const int fiftySeconds = 50000;

//    asiLogger.DebugFormat(this, HubConnectionConstants.RestartEventHasBeenRaised, fiftySeconds);
//    taskAdapter.Delay(() => OnRestart(), fiftySeconds);
//}

//private bool HubConnectionIsAlreadyConnected()
//{
//    return hubConnectionAdapter != null && hubConnectionAdapter.State == ConnectionState.Connected;
//}

//private void HandleException(Exception exception)
//{
//    locationDependentLogger.Error(this, exception.Message, string.Format(HubConnectionConstants.HubConnectionManagerException, exception));
//}

//private void HandleAggregateException(AggregateException aggregateException)
//{
//    var stringBuilder = new StringBuilder();
//    foreach (var innerException in aggregateException.Flatten().InnerExceptions)
//    {
//        stringBuilder.Append(innerException);
//    }

//    locationDependentLogger.Error(this, aggregateException.Message,
//        string.Format(HubConnectionConstants.HubConnectionManagerException, stringBuilder));
//}
//    }
//}