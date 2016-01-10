namespace Domain
{
    using System;

    public class MessageLog : AggregateRoot
    {
        public Guid MessageId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Body { get; set; }
        public string Headers { get; set; }
        public string MessageType { get; set; }
        public int Retries { get; set; }
        public int BrokerId { get; set; }
    }
}