namespace Audit.Controllers
{
    using System;
    using System.Web.Http;

    using Services;

    using Messages.Commands;

    using NServiceBus;

    [Authorize]
    [RoutePrefix("Acknowledgement")]
    public class AcknowledgementController : ApiController
    {
        private readonly IBus bus;

        private readonly ICompletionService completionService;

        public AcknowledgementController(IBus bus, ICompletionService completionService)
        {
            this.bus = bus;
            this.completionService = completionService;
        }

        [Route("")]
        public IHttpActionResult Post(SendAcknowledgementCommand sendAcknowledgementCommand)
        {
            if (!sendAcknowledgementCommand.Success)
            {
                Console.WriteLine("received remote acknowledgment of failed insert. Log and send email");
                return Ok();
            }

            //write to db syncronously - async will not guarentee
            completionService.CompleteCorrelatedMessages(sendAcknowledgementCommand.CorrelationId);

            //using a command as it is in a web thread
            bus.Send(new CompleteAgreementSagaCommand { CorrelationId = sendAcknowledgementCommand.CorrelationId });

            return Ok();
        }
    }
}