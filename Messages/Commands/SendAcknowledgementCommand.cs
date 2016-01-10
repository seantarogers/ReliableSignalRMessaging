namespace Messages.Commands
{
    public class SendAcknowledgementCommand : SignalRAuditMessage
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}