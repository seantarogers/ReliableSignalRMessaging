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