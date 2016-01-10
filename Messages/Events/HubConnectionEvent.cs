namespace Messages.Events
{
    public class HubConnectionEvent : SignalRAuditMessage
    {
        public string ConnectionId { get; set; }
        public string ConnectionEventType { get; set; }
    }
}