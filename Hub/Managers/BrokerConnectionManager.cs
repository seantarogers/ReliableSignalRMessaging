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

            public int NumberOfConnectedBrokers
            {
                get
                {
                    lock (BrokerConnections)
                    {
                        return BrokerConnections.Select(b => b.BrokerId)
                            .Distinct()
                            .Count();
                    }
                }
            }

            public bool IsBrokerConnected(int brokerId)
            {
                lock (BrokerConnections)
                {
                    return BrokerConnections.Any(b => b.BrokerId == brokerId);
                }
            }

            public void AddConnection(string connectionId, int brokerId, DateTime tokenExpiresOn)
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

            public void RemoveConnection(string connectionId)
            {
                lock (BrokerConnections)
                {
                    if (BrokerConnections.Any(b => b.ConnectionId == connectionId))
                    {
                        BrokerConnections.Remove(BrokerConnections.First(b => b.ConnectionId == connectionId));
                    }
                }
            }

            public void ClearBrokerConnections()
            {
                lock (BrokerConnections)
                {
                    BrokerConnections.Clear();
                }
            }

            public bool ActiveBrokerTokenIsDueToExpire(int brokerId)
            {
                lock (BrokerConnections)
                {
                    if (BrokerConnections.All(b => b.BrokerId != brokerId))
                    {
                        throw new ApplicationException("Cannot find Broker in list of connections");
                    }

                    var brokerConnection = BrokerConnections.First(b => b.BrokerId == brokerId && !b.Expired);
                    return brokerConnection.TokenExpiresOn <= DateTime.UtcNow.AddMinutes(5);
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
        }
    }
}