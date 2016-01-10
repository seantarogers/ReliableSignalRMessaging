namespace Messages
{
    using System;

    public abstract class Message
    {
        public Message()
        {
            CreateDate = DateTime.Now;
            Id = Guid.NewGuid();
        }

        public DateTime CreateDate { get; set; }
        public Guid Id { get; set; }
        public Guid CorrelationId { get; set; }
        public int BrokerId { get; set; }
    }
}