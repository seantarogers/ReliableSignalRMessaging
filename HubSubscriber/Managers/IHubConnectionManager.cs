namespace HubSubscriber.Managers
{
    public interface IHubConnectionManager
    {
        bool Start();
        void Stop();
        bool AccessTokenNeedsRefresh();
    }
}