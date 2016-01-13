namespace Messages.Commands
{
    public class SubmitAgreementCommand : Message
    {
        public string AgreementDocumentUrl { get; set; }
        public int AgreementId { get; set; }
    }
}