namespace OnlineBackOffice.Handlers
{
    using System;

    using Messages.Commands;
    using Messages.Events;

    using NServiceBus;

    public class InsertDocumentIntoOnlineBackOfficeCommandHandler :
        IHandleMessages<InsertDocumentIntoOnlineBackOfficeCommand>
    {
        private readonly IBus bus;

        public InsertDocumentIntoOnlineBackOfficeCommandHandler(IBus bus)
        {
            this.bus = bus;
        }

        public void Handle(InsertDocumentIntoOnlineBackOfficeCommand command)
        {
            Console.WriteLine("Write document into back office system hosted in the online data centre.");
            bus.Publish(
                new OnlineDocumentSuccessfullyInsertedEvent
                    {
                        CorrelationId = command.CorrelationId,
                        BrokerId = command.BrokerId
                    });
        }
    }
}