// Server.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Communicator;

namespace CommunicationModule
{
    public class Server : IServerCommunicator
    {
        private TcpListener _listener;
        private bool _isRunning = false;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();
        private int _clientCounter = 0;
        private readonly string _storagePath;

        // Constructor to initialize storage path
        public Server(string storagePath)
        {
            _storagePath = storagePath;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
                Console.WriteLine($"Created storage directory at {_storagePath}");
            }
        }

        /// <summary>
        /// Starts the server by initializing the TcpListener, beginning to listen for client connections,
        /// and handling each client on a separate thread.
        /// </summary>
        /// <param name="serverIP">Not used for server; can be null.</param>
        /// <param name="serverPort">Port number as string on which the server listens.</param>
        /// <returns>Returns "IP:Port" on success, "failure" on failure.</returns>
        public string Start(string serverIP = null, string serverPort = null)
        {
            if (string.IsNullOrEmpty(serverPort))
            {
                Console.WriteLine("Server port must be specified.");
                return "failure";
            }

            if (!int.TryParse(serverPort, out int port))
            {
                Console.WriteLine("Invalid port number.");
                return "failure";
            }

            try
            {
                // Initialize TcpListener to listen on any IP address on the specified port
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _isRunning = true;
                Console.WriteLine($"Server started. Listening on port {port}.");

                // Start a thread to accept clients
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.Start();

                // Get the server's local IP address
                string localIP = GetLocalIPAddress();
                if (localIP == null)
                {
                    Console.WriteLine("Unable to determine local IP address.");
                    return "failure";
                }

                // Return the server address
                return $"{localIP}:{port}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                return "failure";
            }
        }

        /// <summary>
        /// Continuously accepts incoming client connections.
        /// </summary>
        private void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    string clientId = _clientCounter.ToString();
                    _clientCounter++;

                    if (_clients.TryAdd(clientId, client))
                    {
                        Console.WriteLine($"Client connected. Assigned ID: {clientId}");

                        // Send the client its ID
                        NetworkStream stream = client.GetStream();
                        byte[] idBytes = Encoding.UTF8.GetBytes(clientId + "\n");
                        stream.Write(idBytes, 0, idBytes.Length);

                        // Notify subscribed handlers about new client
                        foreach (var handler in _notificationHandlers.Values)
                        {
                            handler.OnClientJoined(client);
                        }

                        // Start a thread to handle this client
                        Thread clientThread = new Thread(() => HandleClient(client, clientId));
                        clientThread.Start();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add client {clientId} to the client list.");
                        client.Close();
                    }
                }
                catch (SocketException se)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"SocketException: {se.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in AcceptClients: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles communication with a connected client.
        /// Receives files from the client and stores them in the designated storage folder.
        /// </summary>
        /// <param name="client">The connected TcpClient.</param>
        /// <param name="clientId">The unique ID assigned to the client.</param>
        private void HandleClient(TcpClient client, string clientId)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                while (_isRunning)
                {
                    // Read the file name
                    string fileName = reader.ReadLine();
                    if (fileName == null)
                        break; // Client disconnected

                    // Read the file size
                    string fileSizeStr = reader.ReadLine();
                    if (!long.TryParse(fileSizeStr, out long fileSize))
                    {
                        Console.WriteLine($"Invalid file size received from client {clientId}.");
                        break;
                    }

                    // Prepare the full path for storing the file
                    string sanitizedFileName = SanitizeFileName(fileName);
                    string fullPath = Path.Combine(_storagePath, $"Client{clientId}_{sanitizedFileName}");

                    // Receive the file data
                    using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        long totalRead = 0;

                        while (totalRead < fileSize)
                        {
                            int bytesToRead = buffer.Length;
                            if (fileSize - totalRead < bytesToRead)
                                bytesToRead = (int)(fileSize - totalRead);

                            int bytesRead = stream.Read(buffer, 0, bytesToRead);
                            if (bytesRead == 0)
                                break; // Client disconnected

                            fs.Write(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                        }

                        if (totalRead == fileSize)
                        {
                            Console.WriteLine($"Received file '{sanitizedFileName}' from client {clientId} ({fileSize} bytes).");
                        }
                        else
                        {
                            Console.WriteLine($"Incomplete file '{sanitizedFileName}' received from client {clientId}.");
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"IO Exception with client {clientId}: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception with client {clientId}: {ex.Message}");
            }
            finally
            {
                RemoveClient(clientId);
                client.Close();
                Console.WriteLine($"Client {clientId} disconnected.");

                // Notify subscribed handlers about client leaving
                foreach (var handler in _notificationHandlers.Values)
                {
                    handler.OnClientLeft(clientId);
                }
            }
        }

        /// <summary>
        /// Retrieves the local IP address of the server.
        /// </summary>
        /// <returns>Local IP address as a string, or null if not found.</returns>
        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving local IP address: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sanitizes the file name to prevent directory traversal attacks and invalid characters.
        /// </summary>
        /// <param name="fileName">Original file name.</param>
        /// <returns>Sanitized file name.</returns>
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        /// <summary>
        /// Stops the server, closes all client connections, and releases resources.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            foreach (var kvp in _clients)
            {
                try
                {
                    kvp.Value.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing client {kvp.Key}: {ex.Message}");
                }
            }

            _clients.Clear();
            Console.WriteLine("Server has been stopped.");
        }

        /// <summary>
        /// Adds a client to the server's client list.
        /// </summary>
        /// <param name="clientId">The unique ID assigned to the client.</param>
        /// <param name="socket">The TcpClient socket.</param>
        public void AddClient(string clientId, TcpClient socket)
        {
            if (_clients.TryAdd(clientId, socket))
            {
                Console.WriteLine($"Client {clientId} added.");
            }
            else
            {
                Console.WriteLine($"Failed to add client {clientId}.");
            }
        }

        /// <summary>
        /// Removes a client from the server's client list.
        /// </summary>
        /// <param name="clientId">The unique ID of the client to remove.</param>
        public void RemoveClient(string clientId)
        {
            if (_clients.TryRemove(clientId, out TcpClient removedClient))
            {
                try
                {
                    removedClient.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing client {clientId}: {ex.Message}");
                }
                Console.WriteLine($"Client {clientId} removed.");
            }
            else
            {
                Console.WriteLine($"Failed to remove client {clientId}.");
            }
        }

        // Implementation of Send method from ICommunicator
        public void Send(string serializedData, string moduleOfPacket, string? destination)
        {
            if (destination != null)
            {
                // Send to specific client
                if (_clients.TryGetValue(destination, out TcpClient client))
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] dataBytes = Encoding.UTF8.GetBytes(serializedData + "\n");
                        stream.Write(dataBytes, 0, dataBytes.Length);
                        Console.WriteLine($"Sent data to client {destination}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending data to client {destination}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Client {destination} not found.");
                }
            }
            else
            {
                // Broadcast to all clients
                foreach (var kvp in _clients)
                {
                    try
                    {
                        NetworkStream stream = kvp.Value.GetStream();
                        byte[] dataBytes = Encoding.UTF8.GetBytes(serializedData + "\n");
                        stream.Write(dataBytes, 0, dataBytes.Length);
                        Console.WriteLine($"Broadcasted data to client {kvp.Key}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting data to client {kvp.Key}: {ex.Message}");
                    }
                }
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
