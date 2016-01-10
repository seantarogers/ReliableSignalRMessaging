namespace Messages.Events
{
    public class DocumentDownloadedEvent : StandardAuditMessage
    {
        public byte[] CompressedDocument { get; set; }
    }
}