namespace HubSubscriber.Managers
{
    using System;

    public class MessageStore : IMessageStore
    {
        public void AddMessageId(Guid messageId)
        {
            //todo write to Esent
            Console.WriteLine("Adding message id to Esent datastore");
        }

        public bool MessageExists(Guid messageId)
        {
            //todo implement Esent
            Console.WriteLine("messageId has not been previously processed");
            return false;
        }
    }
}