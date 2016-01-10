namespace Hub.Managers
{
    using System;

    public interface IBrokerConnectionManager
    {
        bool IsBrokerConnected(int brokerId);

        void AddBroker(string connectionId, int brokerId, DateTime tokenExpiresOn);

        void RemoveBroker(string connectionId);

        bool ActiveBrokerTokenIsDueToExpire(int brokerId);
    }
}