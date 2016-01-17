namespace MessageStore.IntegrationTests
{
    using System;

    public class Message
    {
        public Guid Id { get; set; }

        public DateTime CreateDate { get; set; }
    }
}