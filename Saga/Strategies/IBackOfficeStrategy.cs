namespace Saga.Strategies
{
    using System;

    public interface IBackOfficeStrategy
    {
        bool IsApplicable(int brokerId);

        void SendDocument(byte[] document, Guid correlationId, int brokerId, int agreementId);
    }
}