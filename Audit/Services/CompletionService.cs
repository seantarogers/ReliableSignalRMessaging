namespace Audit.Services
{
    using System;
    using System.Linq;

    using Persistence;

    public class CompletionService : ICompletionService
    {
        private readonly IAuditContext auditContext;

        public CompletionService(IAuditContext auditContext)
        {
            this.auditContext = auditContext;
        }

        public void CompleteCorrelatedMessages(Guid correlationId)
        {
            using (var transaction = auditContext.BeginTransaction())
            {
                var correlatedMessages = auditContext.MessageLog.Where(m => m.CorrelationId == correlationId);

                foreach (var correlatedMessage in correlatedMessages)
                {
                    correlatedMessage.CompletionDate = DateTime.Now;
                }

                auditContext.SaveChanges();
                transaction.Commit();
            }
        }
    }
}