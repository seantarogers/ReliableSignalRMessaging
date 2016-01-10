namespace DocumentDownloader.Handlers
{
    using System;
    using System.Net.Http;

    using Messages.Commands;
    using Messages.Events;

    using MessagingInfrastructure.Services;

    using NServiceBus;

    public class DownloadDocumentCommandHandler : IHandleMessages<DownloadDocumentCommand>
    {
        private readonly IBus bus;
        private readonly ICompressionService compressionService;

        public DownloadDocumentCommandHandler(IBus bus, ICompressionService compressionService)
        {
            this.bus = bus;
            this.compressionService = compressionService;
        }

        public void Handle(DownloadDocumentCommand downloadDocumentCommand)
        {
            var documentBytes = DownloadDocument(downloadDocumentCommand);

            bus.Publish(
                new DocumentDownloadedEvent
                    {
                        CompressedDocument = documentBytes,
                        CorrelationId = downloadDocumentCommand.CorrelationId,
                        BrokerId = downloadDocumentCommand.BrokerId
                    });
        }

        private byte[] DownloadDocument(DownloadDocumentCommand downloadDocumentCommand)
        {
            using (var httpClient = new HttpClient())
            using (var response = httpClient.GetAsync(downloadDocumentCommand.AttachmentUrl).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApplicationException("Could not download document");
                }

                var documentBytes = response.Content.ReadAsByteArrayAsync().Result;
                return compressionService.CompressWithGzip(documentBytes);
            }
        }
    }
}