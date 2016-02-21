namespace HubSubscriber.Services
{
    using System;

    public interface IMessageStoreService
    {
        void AddMessageId(Guid messageId);

        bool MessageExists(Guid messageId);
    }
}