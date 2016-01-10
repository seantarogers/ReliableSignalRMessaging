namespace Saga.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Messages.Commands;
    using Messages.Events;

    using NServiceBus;
    using NServiceBus.Saga;

    using Strategies;

    public class AgreementSaga : Saga<AgreementSagaData>,
                             IAmStartedByMessages<AgreementPlacedEvent>,
                             IHandleMessages<DocumentDownloadedEvent>,
                             IHandleMessages<CompleteAgreementSagaCommand>,
                             IHandleMessages<OnlineDocumentSuccessfullyInsertedEvent>
    {
        private readonly IBus bus;

        private readonly IEnumerable<IBackOfficeStrategy> backOfficeStrategies;

        public AgreementSaga(
            IBus bus, 
            IEnumerable<IBackOfficeStrategy> backOfficeStrategies)
        {
            this.bus = bus;
            this.backOfficeStrategies = backOfficeStrategies;
        }

        public void Handle(AgreementPlacedEvent agreementPlacedEvent)
        {
            SaveSagaData(agreementPlacedEvent);

            bus.Send(
                new DownloadDocumentCommand
                    {
                        BrokerId = agreementPlacedEvent.BrokerId,
                        AttachmentUrl = agreementPlacedEvent.AgreementDocumentUrl,
                        CorrelationId = agreementPlacedEvent.CorrelationId
                    });
        }

        public void Handle(DocumentDownloadedEvent documentDownloadedEvent)
        {
            Data.DocumentDownloaded = DateTime.Now;

            var backOfficeStrategy = backOfficeStrategies.First(b => b.IsApplicable(Data.BrokerId));

            backOfficeStrategy.SendDocument(
                documentDownloadedEvent.CompressedDocument,
                documentDownloadedEvent.CorrelationId,
                Data.BrokerId,
                Data.AgreementId);
        }

        public void Handle(CompleteAgreementSagaCommand completeAgreementSagaCommand)
        {
            MarkAsComplete();
        }

        public void Handle(OnlineDocumentSuccessfullyInsertedEvent onlineDocumentSuccessfullyInsertedEvent)
        {
            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AgreementSagaData> mapper)
        {
            mapper.ConfigureMapping<AgreementPlacedEvent>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<DocumentDownloadedEvent>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<OnlineDocumentSuccessfullyInsertedEvent>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<CompleteAgreementSagaCommand>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
        }

        private void SaveSagaData(AgreementPlacedEvent agreementPlacedEvent)
        {
            Data.BrokerId = agreementPlacedEvent.BrokerId;
            Data.CorrelationId = agreementPlacedEvent.CorrelationId;
            Data.AgreementId = agreementPlacedEvent.AgreementId;
            Data.CreateDate = DateTime.Now;
        }
    }
}