namespace Hub.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace PFIntRemotePublisher.Application.Event.Managers
    {
        public class BrokerConnectionManager : IBrokerConnectionManager
        {
            private static readonly List<BrokerConnection> BrokerConnections;

            static BrokerConnectionManager()
            {
                BrokerConnections = new List<BrokerConnection>();
            }

            public bool IsBrokerConnected(int brokerId)
            {
                lock (BrokerConnections)
                {
                    return BrokerConnections.Any(b => b.BrokerId == brokerId);
                }
            }

            public void AddBroker(string connectionId, int brokerId, DateTime tokenExpiresOn)
            {
                lock (BrokerConnections)
                {
                    if (BrokerConnections.Any(b => b.ConnectionId == connectionId))
                    {
                        return;
                    }

                    ExpireSuperceededConnections(brokerId);

                    BrokerConnections.Add(
                        new BrokerConnection
                            {
                                BrokerId = brokerId,
                                ConnectionId = connectionId,
                                TokenExpiresOn = tokenExpiresOn
                            });
                }
            }

            private static void ExpireSuperceededConnections(int brokerId)
            {
                var superceededBrokerConnections = BrokerConnections.Where(b => b.BrokerId == brokerId);
                foreach (var superceededBrokerConnection in superceededBrokerConnections)
                {
                    superceededBrokerConnection.Expired = true;
                }
            }

            public void RemoveBroker(string connectionId)
            {
                lock (BrokerConnections)
                {
                    if (BrokerConnections.Any(b => b.ConnectionId == connectionId))
                    {
                        BrokerConnections.Remove(BrokerConnections.First(b => b.ConnectionId == connectionId));
                    }
                }
            }

            public bool ActiveBrokerTokenIsDueToExpire(int brokerId)
            {
                lock (BrokerConnections)
                {
                    var brokerConnection = BrokerConnections.First(b => b.BrokerId == brokerId && !b.Expired);
                    return brokerConnection.TokenExpiresOn <= DateTime.UtcNow.AddMinutes(5);
                }
            }
        }
    }
}