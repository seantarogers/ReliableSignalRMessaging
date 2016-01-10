namespace Audit.Handlers
{
    using Factories;

    using Messages;

    using NServiceBus;

    using Persistence;

    public class SuccessfulMessageHandler : IHandleMessages<StandardAuditMessage>
    {
        private readonly IMessageLogFactory messageLogFactory;

        private readonly IAuditContext auditContext;

        private readonly IBus bus;
        
        public SuccessfulMessageHandler(
            IAuditContext auditContext,
            IBus bus,
            IMessageLogFactory messageLogFactory)
        {
            this.auditContext = auditContext;
            this.bus = bus;
            this.messageLogFactory = messageLogFactory;
        }

        public void Handle(StandardAuditMessage message)
        {
            var messageLog = messageLogFactory.Create(message, bus.CurrentMessageContext.Headers);
            auditContext.MessageLog.Add(messageLog);
            auditContext.SaveChanges();
        }
    }
}