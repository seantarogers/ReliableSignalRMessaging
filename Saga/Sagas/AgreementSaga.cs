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
                             IAmStartedByMessages<SubmitAgreementCommand>,
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

        public void Handle(SubmitAgreementCommand submitAgreementCommand)
        {
            SaveSagaData(submitAgreementCommand);

            bus.Send(
                new DownloadDocumentCommand
                    {
                        BrokerId = submitAgreementCommand.BrokerId,
                        AttachmentUrl = submitAgreementCommand.AgreementDocumentUrl,
                        CorrelationId = submitAgreementCommand.CorrelationId
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
            mapper.ConfigureMapping<SubmitAgreementCommand>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<DocumentDownloadedEvent>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<OnlineDocumentSuccessfullyInsertedEvent>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
            mapper.ConfigureMapping<CompleteAgreementSagaCommand>(s => s.CorrelationId)
                .ToSaga(m => m.CorrelationId);
        }

        private void SaveSagaData(SubmitAgreementCommand submitAgreementCommand)
        {
            Data.BrokerId = submitAgreementCommand.BrokerId;
            Data.CorrelationId = submitAgreementCommand.CorrelationId;
            Data.AgreementId = submitAgreementCommand.AgreementId;
            Data.CreateDate = DateTime.Now;
        }
    }
}