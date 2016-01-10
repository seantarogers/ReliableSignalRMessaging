namespace Audit.Handlers
{
    using Domain;

    using Messages.Events;

    using NServiceBus;

    using Persistence;

    public class HubConnectionEventHandler : IHandleMessages<HubConnectionEvent>
    {
        private readonly IAuditContext auditContext;

        public HubConnectionEventHandler(IAuditContext auditContext)
        {
            this.auditContext = auditContext;
        }

        public void Handle(HubConnectionEvent hubConnectionEvent)
        {
            using (var transaction = auditContext.BeginTransaction())
            {
                auditContext.HubConnectionLog.Add(
                    new HubConnectionLog()
                        {
                            BrokerId = hubConnectionEvent.BrokerId,
                            ConnectionEventType = hubConnectionEvent.ConnectionEventType,
                            ConnectionId = hubConnectionEvent.ConnectionId,
                            CreateDate = hubConnectionEvent.CreateDate
                        });

                auditContext.SaveChanges();

                transaction.Commit();
            }
        }
    }
}