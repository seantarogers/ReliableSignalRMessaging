namespace Messages.Commands
{
    public class DownloadDocumentCommand : StandardAuditMessage
    {
        public string AttachmentUrl { get; set; }
    }
}