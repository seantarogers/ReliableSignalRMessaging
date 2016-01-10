namespace Domain
{
    public class HubConnectionLog : AggregateRoot
    {
        public int BrokerId { get; set; }
        public string ConnectionEventType { get; set; }
        public string ConnectionId { get; set; }
    }
}