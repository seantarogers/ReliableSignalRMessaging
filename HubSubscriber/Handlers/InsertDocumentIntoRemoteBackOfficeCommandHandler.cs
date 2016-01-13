namespace HubSubscriber.Handlers
{
    using System.Transactions;

    using Services;

    using Managers;

    using Messages.Commands;

    using NServiceBus;

    public class InsertDocumentIntoRemoteBackOfficeCommandHandler :
        IHandleMessages<InsertDocumentIntoRemoteBackOfficeCommand>
    {
        private readonly IBus bus;

        private readonly IMessageStore messageStore;

        private readonly IBackOfficeService backOfficeService;
        
        public InsertDocumentIntoRemoteBackOfficeCommandHandler(
            IBus bus, 
            IMessageStore messageStore, 
            IBackOfficeService backOfficeService)
        {
            this.bus = bus;
            this.messageStore = messageStore;
            this.backOfficeService = backOfficeService;
        }

        public void Handle(InsertDocumentIntoRemoteBackOfficeCommand insertCommand)
        {
            //note: using commands so that the remote bus does not need a persistence layer
            if (messageStore.MessageExists(insertCommand.Id))
            {
                bus.Send(new SendAcknowledgementCommand
                             {
                                 CorrelationId = insertCommand.CorrelationId, Success = true 
                             });
                return;
            }

            using (var transactionScope = new TransactionScope())
            {
                messageStore.AddMessageId(insertCommand.Id);
                var result = backOfficeService.InsertDocument();

                if (result)
                {
                    bus.Send(new SendAcknowledgementCommand
                                  {
                                      CorrelationId = insertCommand.CorrelationId, Success = true
                                  });

                    //commit messageid to message store if successfully inserted to back office.
                    transactionScope.Complete();
                    return;
                }
            }

            bus.Send(
                new SendAcknowledgementCommand
                    {
                        CorrelationId = insertCommand.CorrelationId,
                        Success = false,
                        ErrorMessage = "Failed to write to back office with error code xyz"
                    });
        }
    }
}

