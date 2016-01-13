namespace Hub.Managers
{
    using System;

    public interface IBrokerConnectionManager
    {
        void ClearBrokerConnections();
        int NumberOfConnectedBrokers { get; }
        bool IsBrokerConnected(int brokerId);
        void AddConnection(string connectionId, int brokerId, DateTime tokenExpiresOn);
        void RemoveConnection(string connectionId);
        bool ActiveBrokerTokenIsDueToExpire(int brokerId);
    }
}