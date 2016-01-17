using System;

namespace HubSubscriber.UnitTests
{
    using Handlers;

    using HubSubscriber.Services;

    using Managers;

    using Messages.Commands;

    using MessageStore;

    using Moq;

    using NServiceBus;

    using NUnit.Framework;

    [TestFixture]
    public class InsertDocumentIntoRemoteBackOfficeCommandHandlerTests
    {
        private Mock<IBus> bus;

        private Mock<IBackOfficeService> backOfficeService;

        private Mock<IMessageStore> messageStore;

        private InsertDocumentIntoRemoteBackOfficeCommandHandler sut;

        [SetUp]
        public void SetUp()
        {
            bus = new Mock<IBus>();
            backOfficeService = new Mock<IBackOfficeService>();
            messageStore = new Mock<IMessageStore>();

            sut = new InsertDocumentIntoRemoteBackOfficeCommandHandler(
                bus.Object,
                messageStore.Object,
                backOfficeService.Object);
        }

        [Test]
        public void Handle_WhenMessageHasPreviouslyBeenProcessed_ShouldSendAcknowledgement_AndReturn()
        {
            // Arrange
            var command = new InsertDocumentIntoRemoteBackOfficeCommand { CorrelationId = Guid.NewGuid(), Id = Guid.NewGuid() };
            messageStore.Setup(s => s.MessageExists(command.Id))
                .Returns(true);

            // Act
            sut.Handle(command);

            // Assert
            bus.Verify(
                b => b.Send(It.Is<SendAcknowledgementCommand>(s => s.CorrelationId == command.CorrelationId && s.Success)),
                Times.Once());

            messageStore.Verify(m => m.AddMessage(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void Handle_WhenMessageHasNotPreviouslyBeenProcessed_AndBackOfficeInsertIsSuccessfull_ShouldAddGuidToMessageStore_AndSendAcknowledgement()
        {
            // Arrange
            var command = new InsertDocumentIntoRemoteBackOfficeCommand { CorrelationId = Guid.NewGuid(), Id = Guid.NewGuid()};
            messageStore.Setup(s => s.MessageExists(command.Id))
                .Returns(false);
            backOfficeService.Setup(s => s.InsertDocument())
                .Returns(true);
            
            // Act
            sut.Handle(command);

            // Assert
            bus.Verify(
                b => b.Send(It.Is<SendAcknowledgementCommand>(s => s.CorrelationId == command.CorrelationId  && s.Success)),
                Times.Once());

            messageStore.Verify(m => m.AddMessage(It.IsAny<Guid>()));
        }

        [Test]
        public void Handle_WhenBackOfficeInsertFails_ShouldSendFailureAcknowledgement()
        {
            // Arrange
            var command = new InsertDocumentIntoRemoteBackOfficeCommand
                              {
                                  CorrelationId = Guid.NewGuid(),
                                  Id = Guid.NewGuid()
                              };
            messageStore.Setup(s => s.MessageExists(command.Id))
                .Returns(false);
            backOfficeService.Setup(s => s.InsertDocument())
                .Returns(false);
            
            // Act
            sut.Handle(command);

            // Assert
            bus.Verify(
                b =>
                b.Send(
                    It.Is<SendAcknowledgementCommand>(
                        s =>
                        s.CorrelationId == command.CorrelationId && !s.Success
                        && s.ErrorMessage == "Failed to write to back office with error code xyz")),
                Times.Once());
        }
    }
}
