namespace Saga.Strategies
{
    using System;

    using Messages.Commands;

    using NServiceBus;

    public class RemoteStrategy : IBackOfficeStrategy
    {
        private readonly IBus bus;

        public RemoteStrategy(IBus bus)
        {
            this.bus = bus;
        }

        public bool IsApplicable(int brokerId)
        {
            return brokerId == 123;
        }

        public void SendDocument(byte[] document, Guid correlationId, int brokerId, int agreementId)
        {
            bus.Send(new InsertDocumentIntoRemoteBackOfficeCommand {
                        Document = document,
                        AgreementId = agreementId,
                        CorrelationId = correlationId,
                        BrokerId = brokerId
                    });
        }
    }
}