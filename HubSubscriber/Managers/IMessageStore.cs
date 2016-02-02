namespace HubSubscriber.Managers
{
    using System;

    public interface IMessageStore
    {
        void AddMessageId(Guid messageId);

        bool MessageExists(Guid messageId);
    }
}