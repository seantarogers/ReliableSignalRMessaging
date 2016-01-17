namespace MessageStore
{
    using System;

    public interface IMessageStore
    {
        void AddMessage(Guid messageId);
        bool MessageExists(Guid messageId);
    }
}