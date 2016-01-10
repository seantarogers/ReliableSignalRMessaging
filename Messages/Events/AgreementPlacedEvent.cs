namespace Messages.Events
{
    public class AgreementPlacedEvent : StandardAuditMessage
    {
        public string AgreementDocumentUrl { get; set; }
        public int AgreementId { get; set; }
    }
}