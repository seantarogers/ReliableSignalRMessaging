namespace Hub.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;

    using Messages;
    using Messages.Commands;
    using Messages.Events;

    using NServiceBus;

    using Persistence;

    [Authorize]
    [RoutePrefix("Acknowledgement")]
    public class AcknowledgementController : ApiController
    {
        private readonly IBus bus;

        private readonly IAuditContext auditContext;

        public AcknowledgementController(
            IBus bus, 
            IAuditContext auditContext)
        {
            this.bus = bus;
            this.auditContext = auditContext;
        }

        [Route("")]
        public IHttpActionResult Post(SendAcknowledgementCommand sendAcknowledgementCommand)
        {
            //write to db syncronously - async will not guarentee
            AcknowledgeCorrelatedMessages(sendAcknowledgementCommand);

            //using a different event here as the subscriber is already subscribing to the one above
            bus.Send(
                new CompleteAgreementSagaCommand()
                    {
                        CorrelationId =
                            sendAcknowledgementCommand.CorrelationId
                    });

            return Ok();
        }
        
        private void AcknowledgeCorrelatedMessages(Message remoteDocumentSuccessfullyInsertedEvent)
        {
            using (var transaction = auditContext.BeginTransaction())
            {
                var correlatedMessages =
                    auditContext.MessageLog.Where(
                        m => m.CorrelationId == remoteDocumentSuccessfullyInsertedEvent.CorrelationId);

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