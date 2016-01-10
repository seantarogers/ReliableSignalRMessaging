namespace Saga.Sagas
{
    using System;

    using NServiceBus.Saga;

    public class AgreementSagaData : IContainSagaData
    {
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }
        public virtual int BrokerId { get; set; }
        public virtual int AgreementId { get; set; }
        public virtual Guid CorrelationId { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public virtual DateTime? DocumentDownloaded { get; set; }
        public virtual DateTime? DocumentSuccessfullyInserted { get; set; }
    }
}
