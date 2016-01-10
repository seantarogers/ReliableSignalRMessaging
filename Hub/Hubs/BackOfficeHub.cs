namespace Hub.Hubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Contracts;

    using Managers;

    using Messages;
    using Messages.Commands;
    using Messages.Events;

    using MessagingInfrastructure.Services;

    using Microsoft.AspNet.SignalR;

    using NServiceBus;

    using Persistence;

    [Authorize]
    public class BackOfficeHub : Hub<IBackOfficeHubClient>, IBackOfficeHub
    {
        private readonly IBus bus;
        private readonly IBrokerConnectionManager brokerConnectionManager;

        private readonly IJsonSerializer jsonSerializer;

        private readonly IAuditContext auditContext;
        
        public BackOfficeHub(
            IBus bus, 
            IBrokerConnectionManager brokerConnectionManager,
            IAuditContext auditContext, 
            IJsonSerializer jsonSerializer)
        {
            this.bus = bus;
            this.brokerConnectionManager = brokerConnectionManager;
            this.auditContext = auditContext;
            this.jsonSerializer = jsonSerializer;
        }

        //Note: no implementation of publish methods as it is done via the hub context...

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {

                var connectionEvent = CreateConnectionEvent("Disconnected");
                bus.Publish(connectionEvent);

                brokerConnectionManager.RemoveBroker(Context.ConnectionId);
            }
            catch (Exception exception)
            {
                Console.WriteLine("There has been an exception: {0}", exception);
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            try
            {
                var connectionEvent = CreateConnectionEvent("Reconnected");
                bus.Publish(connectionEvent);

                AddBrokerToConnections();
            }
            catch (Exception exception)
            {
                Console.WriteLine("There has been an exception: {0}", exception);
            }
            return base.OnReconnected();
        }

        public override Task OnConnected()
        {
            try
            {
                var connectionEvent = CreateConnectionEvent("Connected");
                bus.Publish(connectionEvent);
                AddBrokerToConnections();
                ReplayUnacknowledgedMessagesSynchronously();
            }
            catch (Exception exception)
            {
                Console.WriteLine("There has been an exception: {0}", exception);
            }

            return base.OnConnected();
        }

        private void ReplayUnacknowledgedMessagesSynchronously()
        {
           var unacknowledgedMessages = GetUnacknowledgedMessages();

            foreach (var insertDocumentIntoRemoteBackOfficeCommand in unacknowledgedMessages)
            {
                // i know the client is connected as we are in the middle of a connection event
                // so no need to check if they are in the dictionary
                ResendUnacknowledgedMessageToRemoteClient(insertDocumentIntoRemoteBackOfficeCommand);
                UpdateMessageRetries(insertDocumentIntoRemoteBackOfficeCommand);
            }
        }

        private void UpdateMessageRetries(Message replayedMessage)
        {
            using (var transaction = auditContext.BeginTransaction())
            {
                var messageToUpdate = auditContext.MessageLog.First(a => a.BrokerId == replayedMessage.BrokerId);

                messageToUpdate.Retries ++;
                auditContext.SaveChanges();
                transaction.Commit();
            }
        }

        private void ResendUnacknowledgedMessageToRemoteClient(
            InsertDocumentIntoRemoteBackOfficeCommand insertDocumentIntoRemoteBackOfficeCommand)
        {
            var brokerId = GetBrokerId().ToString();
            Clients.User(brokerId).InsertDocument(insertDocumentIntoRemoteBackOfficeCommand);
        }

        private IEnumerable<InsertDocumentIntoRemoteBackOfficeCommand> GetUnacknowledgedMessages()
        {
            var brokerId = GetBrokerId();
            var messageType = typeof(InsertDocumentIntoRemoteBackOfficeCommand).ToString();
            var maxiumDeliveryDate = DateTime.Today.AddDays(1);

            var unacknowledgedMessages =
                auditContext.MessageLog.Where(
                    m =>
                    m.BrokerId == brokerId && m.CompletionDate == null && 
                    m.Retries < 10 && 
                    m.MessageType == messageType && 
                    m.CreateDate <= maxiumDeliveryDate)
                    .ToList();

            return
                unacknowledgedMessages.Select(
                    unacknowledgedMessage =>
                    jsonSerializer.Deserialize<InsertDocumentIntoRemoteBackOfficeCommand>(unacknowledgedMessage.Body))
                    .ToList();
        }

        private void AddBrokerToConnections()
        {
            var claimsPrincipal = (ClaimsPrincipal)Context.User;
            var brokerId = int.Parse(claimsPrincipal.Identity.Name);
            var tokenExpiresOn = GetTokenExpiresOnDateFromClaims(claimsPrincipal);
            var connectionId = Context.ConnectionId;
            brokerConnectionManager.AddBroker(connectionId, brokerId, tokenExpiresOn);
        }

        private static DateTime GetTokenExpiresOnDateFromClaims(ClaimsPrincipal claimsPrincipal)
        {
            return DateTime.UtcNow.AddMinutes(
                int.Parse(
                    claimsPrincipal.Claims.First(c => c.Type == "ExpiresOn")
                        .Value));
        }

        private HubConnectionEvent CreateConnectionEvent(string connectionEventType)
        {
            return new HubConnectionEvent
                       {
                           BrokerId = GetBrokerId(),
                           ConnectionEventType = connectionEventType,
                           ConnectionId = Context.ConnectionId
                       };
        }

        private int GetBrokerId()
        {
            return int.Parse(Context.User.Identity.Name);
        }
    }
}
