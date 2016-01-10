namespace Hub.Handlers
{
    using System;
    using System.Threading.Tasks;

    using Contracts;

    using Hubs;
    using Managers;

    using Messages;
    using Messages.Commands;

    using Microsoft.AspNet.SignalR;

    using NServiceBus;

    public class InsertDocumentIntoRemoteBackOfficeCommandHandler : IHandleMessages<InsertDocumentIntoRemoteBackOfficeCommand>
    {
        private readonly IBrokerConnectionManager brokerConnectionManager;
        
        public InsertDocumentIntoRemoteBackOfficeCommandHandler(
            IBrokerConnectionManager brokerConnectionManager)
        {
            this.brokerConnectionManager = brokerConnectionManager;
            
        }

        public void Handle(InsertDocumentIntoRemoteBackOfficeCommand command)
        {
            //todo - currently if broker is not connected, the message will not be stored in audit
            //and will not get auto replayed.
            //should we save syncronously here - but i do not want duplicates.
            if (!brokerConnectionManager.IsBrokerConnected(command.BrokerId))
            {
                throw new ApplicationException(
                    string.Format("Broker: {0} is not connected to Hub. Try again later", command.BrokerId));
            }

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<BackOfficeHub, IBackOfficeHubClient>();
            hubContext.Clients.User(command.BrokerId.ToString()).InsertDocument(command);

            ThrottlePublishOfNextMessageIfTokenIsAboutToExpire(command);
        }

        private void ThrottlePublishOfNextMessageIfTokenIsAboutToExpire(Message command)
        {
            //this may not be needed now as we have auto retries, but i think it is possibly an interesting idea, 
            //so i have left it in.

            //if token is about to expire for the client then the message just sent will trigger
            //a reconnection, so the subsequent message should not be sent immediately or it will be 
            //lost and have to be replayed
            if (brokerConnectionManager.ActiveBrokerTokenIsDueToExpire(command.BrokerId))
            {
                Task.Delay(new TimeSpan(0, 0, 15))
                    .Wait();
            }
        }
    }
}