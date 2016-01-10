namespace Audit.Handlers
{
    using Services;

    using Messages;

    using NServiceBus;

    public class CompletionMessageHandler : IHandleMessages<CompletionAuditMessage>
    {
        private readonly ICompletionService completionService;

        public CompletionMessageHandler(ICompletionService completionService)
        {
            this.completionService = completionService;            
        }

        public void Handle(CompletionAuditMessage message)
        {
            completionService.CompleteCorrelatedMessages(message.CorrelationId);
        }
    }
}