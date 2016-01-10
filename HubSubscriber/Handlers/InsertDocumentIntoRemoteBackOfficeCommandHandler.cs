namespace HubSubscriber.Handlers
{
    using System.Transactions;

    using Managers;

    using Messages.Commands;

    using NServiceBus;

    public class InsertDocumentIntoRemoteBackOfficeCommandHandler :
        IHandleMessages<InsertDocumentIntoRemoteBackOfficeCommand>
    {
        private readonly IBus bus;

        private readonly IMessageStore messageStore;

        public InsertDocumentIntoRemoteBackOfficeCommandHandler(
            IBus bus, 
            IMessageStore messageStore)
        {
            this.bus = bus;
            this.messageStore = messageStore;
        }

        public void Handle(InsertDocumentIntoRemoteBackOfficeCommand insertCommand)
        {
            //note: using commands so that the remote bus does not need a persistence layer

            if (messageStore.MessageExists(insertCommand.Id))
            {
                bus.Send(new SendAcknowledgementCommand { CorrelationId = insertCommand.CorrelationId, Success = true });
                return;
            }

            using (var transactionScope = new TransactionScope())
            {
                messageStore.AddMessageId(insertCommand.Id);

                var result = WriteDocumentIntoBackOffice();

                if (result)
                {
                    bus.Send(
                        new SendAcknowledgementCommand { CorrelationId = insertCommand.CorrelationId, Success = true });
                    transactionScope.Complete();
                    return;
                }
            }

            bus.Send(new SendAcknowledgementCommand
                    {
                        CorrelationId = insertCommand.CorrelationId,
                        Success = false,
                        ErrorMessage = "Failed to write to back office with error code xyz"
                    });
        }

        private static bool WriteDocumentIntoBackOffice()
        {
            return true;
        }
    }
}

