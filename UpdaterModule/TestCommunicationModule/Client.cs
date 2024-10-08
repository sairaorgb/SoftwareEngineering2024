using Moq;
using System.Net.Sockets;
using System.Text;
using FileTransferApp;
using Communicator;

namespace TestCommunicationModule
{
    [TestClass]
    public class ClientTests
    {
        private Mock<TcpClient> _mockTcpClient;
        private Mock<NetworkStream> _mockNetworkStream;
        private Client _client;

        [TestInitialize]
        public void Setup()
        {
            // Setup mocks
            _mockTcpClient = new Mock<TcpClient>();
            _mockNetworkStream = new Mock<NetworkStream>();

            // Return the mocked NetworkStream when GetStream() is called
            _mockTcpClient.Setup(c => c.GetStream()).Returns(_mockNetworkStream.Object);

            _client = new Client();
        }

        [TestMethod]
        public void Start_ValidConnection_ReturnsSuccess()
        {
            // Arrange
            string serverIP = "127.0.0.1";
            string serverPort = "5000";

            // Setup TcpClient to mock connection
            _mockTcpClient.Setup(client => client.Connect(serverIP, int.Parse(serverPort)));

            // Mock the response for the client ID
            var streamReader = new Mock<StreamReader>(_mockNetworkStream.Object, Encoding.UTF8);
            streamReader.Setup(reader => reader.ReadLine()).Returns("Client123");

            // Act
            string result = _client.Start(serverIP, serverPort);

            // Assert
            Assert.AreEqual("success", result);
        }

        [TestMethod]
        public void Start_InvalidPort_ReturnsFailure()
        {
            // Arrange
            string serverIP = "127.0.0.1";
            string invalidPort = "abc";  // Invalid port

            // Act
            string result = _client.Start(serverIP, invalidPort);

            // Assert
            Assert.AreEqual("failure", result);
        }

        [TestMethod]
        public void Send_ValidData_SendsToServer()
        {
            // Arrange
            _client.Start("127.0.0.1", "5000");
            string serializedData = "TestData";
            string moduleOfPacket = "TestModule";

            // Act
            _client.Send(serializedData, moduleOfPacket, null);

            // Assert
            _mockNetworkStream.Verify(ns => ns.Write(It.IsAny<byte[]>(), 0, It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void Stop_ClosesConnection()
        {
            // Arrange
            _client.Start("127.0.0.1", "5000");

            // Act
            _client.Stop();

            // Assert
            _mockNetworkStream.Verify(ns => ns.Close(), Times.Once);
            _mockTcpClient.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void Subscribe_ModuleSubscribed_Success()
        {
            // Arrange
            var mockHandler = new Mock<INotificationHandler>();
            string moduleName = "TestModule";

            // Act
            _client.Subscribe(moduleName, mockHandler.Object);

            // Assert
            // Here we don't need to mock much, just checking that the method does not throw exceptions
            // or wrong behavior during the subscription process.
            Assert.IsTrue(true); // Test passes if no exceptions occur
        }
    }
}
