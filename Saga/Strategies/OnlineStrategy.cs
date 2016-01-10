namespace Saga.Strategies
{
    using System;

    using Messages.Commands;

    using NServiceBus;

    public class OnlineStrategy : IBackOfficeStrategy
    {
        private readonly IBus bus;

        public OnlineStrategy(IBus bus)
        {
            this.bus = bus;
        }

        public bool IsApplicable(int brokerId)
        {
            return brokerId == 456;
        }

        public void SendDocument(byte[] document, Guid correlationId, int brokerId, int agreementId)
        {
            bus.Send(
                new InsertDocumentIntoOnlineBackOfficeCommand
                    {
                        Document = document,
                        AgreementId = agreementId,
                        CorrelationId = correlationId,
                        BrokerId = brokerId
                    });
        }
    }
}