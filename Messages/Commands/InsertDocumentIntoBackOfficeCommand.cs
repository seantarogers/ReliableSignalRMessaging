namespace Messages.Commands
{
    public abstract class InsertDocumentIntoBackOfficeCommand : StandardAuditMessage
    {
        public int AgreementId { get; set; }
        public byte[] Document { get; set; }
    }
}