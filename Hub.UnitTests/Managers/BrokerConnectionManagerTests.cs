namespace Hub.UnitTests.Managers
{
    using System;

    using Hub.Managers;
    using Hub.Managers.PFIntRemotePublisher.Application.Event.Managers;

    using NUnit.Framework;

    [TestFixture]
    public class BrokerConnectionManagerTests
    {
        private IBrokerConnectionManager sut;

        [SetUp]
        public void SetUp()
        {
            sut = new BrokerConnectionManager();
            sut.ClearBrokerConnections();
        }

        [Test]
        public void AddConnection_WhenConnectionIsAlreadyInList_ShouldNotAddDuplicateConnection()
        {
            // Arrange
            // Act
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));

            // Assert
            Assert.AreEqual(sut.NumberOfConnectedBrokers, 1);
        }

        [Test]
        public void AddConnection_WhenBrokerIsAlreadyInList_ShouldAddNewBrokerConnection()
        {
            // Arrange
            var sut = new BrokerConnectionManager();

            // Act
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("conn2", 1, new DateTime(2010, 10, 10, 10, 10, 10));

            // Assert
            Assert.AreEqual(sut.NumberOfConnectedBrokers, 1);
        }

        [Test]
        public void IsBrokerConnected_WhenBrokerIsNotInList_ShouldReturnFalse()
        {
            // Arrange
            var sut = new BrokerConnectionManager();
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));

            // Act
            var result = sut.IsBrokerConnected(2);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsBrokerConnected_WhenBrokerIsInList_ShouldReturnTrue()
        {
            // Arrange
            var sut = new BrokerConnectionManager();
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));

            // Act
            var result = sut.IsBrokerConnected(1);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void RemoveConnection_WhenBrokerIsInList_ShouldRemoveBroker()
        {
            var sut = new BrokerConnectionManager();

            // Act
            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("conn2", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.RemoveConnection("conn1");

            // Assert
            Assert.AreEqual(sut.NumberOfConnectedBrokers, 1);
        }

        [Test]
        public void ActiveBrokerTokenIsDueToExpire_WhenMostRecentConnectionsTokenHasLessThan5Minutes_ShouldReturnTrue()
        {
            var sut = new BrokerConnectionManager();

            sut.AddConnection("oldconn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("oldconn2", 1, new DateTime(2011, 10, 10, 10, 10, 10));
            sut.AddConnection("newestConn", 1, DateTime.Now.AddMinutes(2));

            // Act
            var result = sut.ActiveBrokerTokenIsDueToExpire(1);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ActiveBrokerTokenIsDueToExpire_WhenMostRecentConnectionsTokenHasMoreThan5Minutes_ShouldReturnFalse()
        {
            var sut = new BrokerConnectionManager();

            sut.AddConnection("oldconn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("oldconn2", 1, new DateTime(2011, 10, 10, 10, 10, 10));
            sut.AddConnection("newestConn", 1, DateTime.Now.AddMinutes(6));

            // Act
            var result = sut.ActiveBrokerTokenIsDueToExpire(1);

            // Assert
            Assert.False(result);
        }

        [Test]
        public void ActiveBrokerTokenIsDueToExpire_WhenBrokerIsNotInList_ShouldThrowApplicationException()
        {
            var sut = new BrokerConnectionManager();

            sut.AddConnection("oldconn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("oldconn2", 1, new DateTime(2011, 10, 10, 10, 10, 10));
            sut.AddConnection("newestConn", 1, DateTime.Now.AddMinutes(6));

            // Act
            var exception = Assert.Throws<ApplicationException>(() => sut.ActiveBrokerTokenIsDueToExpire(2));

            // Assert
            Assert.AreEqual("Cannot find Broker in list of connections", exception.Message);
        }

        [Test]
        public void NumberOfConnectionBrokers_ShouldReturnNumberOfConnectedBrokers()
        {
            var sut = new BrokerConnectionManager();

            sut.AddConnection("conn1", 1, new DateTime(2010, 10, 10, 10, 10, 10));
            sut.AddConnection("conn2", 2, new DateTime(2011, 10, 10, 10, 10, 10));
            sut.AddConnection("conn", 1, DateTime.Now.AddMinutes(6));

            // Act
            var result = sut.NumberOfConnectedBrokers;

            // Assert
            Assert.AreEqual(result, 2);
        }
    }
}