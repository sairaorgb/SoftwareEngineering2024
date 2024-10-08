// Client.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Communicator;

namespace CommunicationModule
{
    public class Client : IClientCommunicator
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _clientId = "";
        private bool _isConnected = false;

        // Implementation of Start method from ICommunicator
        public string Start(string serverIP = null, string serverPort = null)
        {
            if (string.IsNullOrEmpty(serverIP) || string.IsNullOrEmpty(serverPort))
            {
                Console.WriteLine("Server IP and port must be specified for client.");
                return "failure";
            }

            if (!int.TryParse(serverPort, out int port))
            {
                Console.WriteLine("Invalid port number.");
                return "failure";
            }

            try
            {
                _client = new TcpClient();
                _client.Connect(serverIP, port);
                _stream = _client.GetStream();
                _isConnected = true;
                Console.WriteLine($"Connected to server at {serverIP}:{port}.");

                // Read the assigned client ID from server
                StreamReader reader = new StreamReader(_stream, Encoding.UTF8);
                _clientId = reader.ReadLine();
                if (_clientId == null)
                {
                    Console.WriteLine("Failed to receive client ID from server.");
                    return "failure";
                }
                Console.WriteLine($"Received Client ID: {_clientId}");

                // Notify subscribed handlers about client join
                foreach (var handler in _notificationHandlers.Values)
                {
                    handler.OnClientJoined(_client);
                }

                return "success";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
                return "failure";
            }
        }

        /// <summary>
        /// Sends all files in the specified folder to the server.
        /// </summary>
        /// <param name="folderPath">Path to the folder containing files to send.</param>
        public void SendFiles(string folderPath)
        {
            if (!_isConnected)
            {
                Console.WriteLine("Not connected to the server.");
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Specified folder does not exist.");
                return;
            }

            string[] files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    long fileSize = new FileInfo(file).Length;

                    // Send file name
                    byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName + "\n");
                    _stream.Write(fileNameBytes, 0, fileNameBytes.Length);

                    // Send file size
                    byte[] fileSizeBytes = Encoding.UTF8.GetBytes(fileSize.ToString() + "\n");
                    _stream.Write(fileSizeBytes, 0, fileSizeBytes.Length);

                    // Send file data
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            _stream.Write(buffer, 0, bytesRead);
                        }
                    }

                    Console.WriteLine($"Sent file '{fileName}' to server.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending file '{file}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Stops the client, closes the connection, and releases resources.
        /// </summary>
        public void Stop()
        {
            if (_isConnected)
            {
                _stream.Close();
                _client.Close();
                _isConnected = false;
                Console.WriteLine("Disconnected from server.");

                // Notify subscribed handlers about client leaving
                foreach (var handler in _notificationHandlers.Values)
                {
                    handler.OnClientLeft(_clientId);
                }
            }
        }

        /// <summary>
        /// Adds a client to the client's client list.
        /// Not applicable for client; method left empty.
        /// </summary>
        /// <param name="clientId">The client Id.</param>
        /// <param name="socket">The TcpClient socket.</param>
        public void AddClient(string clientId, TcpClient socket)
        {
            // Not applicable for client
            Console.WriteLine("AddClient method is not applicable for client.");
        }

        /// <summary>
        /// Removes a client from the client's client list.
        /// Not applicable for client; method left empty.
        /// </summary>
        /// <param name="clientId">The client Id.</param>
        public void RemoveClient(string clientId)
        {
            // Not applicable for client
            Console.WriteLine("RemoveClient method is not applicable for client.");
        }

        /// <summary>
        /// Sends data to the server or other clients.
        /// For this implementation, it's primarily used to send data to the server.
        /// </summary>
        /// <param name="serializedData">The serialized data to send.</param>
        /// <param name="moduleOfPacket">Name of the module sending the data.</param>
        /// <param name="destination">Destination client ID or null for server.</param>
        public void Send(string serializedData, string moduleOfPacket, string? destination)
        {
            if (!_isConnected)
            {
                Console.WriteLine("Not connected to the server.");
                return;
            }

            try
            {
                // For simplicity, ignoring moduleOfPacket and destination in this implementation
                byte[] dataBytes = Encoding.UTF8.GetBytes(serializedData + "\n");
                _stream.Write(dataBytes, 0, dataBytes.Length);
                Console.WriteLine("Sent data to server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");
            }
        }

        // Implementation of Subscribe method from ICommunicator
        private ConcurrentDictionary<string, INotificationHandler> _notificationHandlers = new ConcurrentDictionary<string, INotificationHandler>();

        public void Subscribe(string moduleName, INotificationHandler notificationHandler, bool isHighPriority = false)
        {
            if (_notificationHandlers.TryAdd(moduleName, notificationHandler))
            {
                Console.WriteLine($"Module '{moduleName}' subscribed for notifications.");
            }
            else
            {
                Console.WriteLine($"Module '{moduleName}' is already subscribed.");
            }
        }

        // Implementation of OnDataReceived from INotificationHandler
        public void OnDataReceived(string serializedData)
        {
            // This method can be implemented based on specific requirements
            // For now, it's left empty
        }

        // Implementation of OnClientJoined from INotificationHandler
        public void OnClientJoined(TcpClient socket)
        {
            // This method can be implemented based on specific requirements
            // For now, it's left empty
        }

        // Implementation of OnClientLeft from INotificationHandler
        public void OnClientLeft(string clientId)
        {
            // This method can be implemented based on specific requirements
            // For now, it's left empty
        }
    }
}
