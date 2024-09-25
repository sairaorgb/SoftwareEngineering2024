using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{

    public interface INotificationHandler
    {
        /// <summary>
        /// Called when data of a particular module appears in the receiving queue
        /// </summary>
        public void OnDataReceived(string serializedData);

        /// <summary>
        /// Called on the server when a new client joins
        /// </summary>
        public void OnClientJoined(TcpClient socket)
        { }

        /// <summary>
        /// Called on the server when a client leaves
        /// </summary>
        public void OnClientLeft(string clientId)
        { }
    }

    public interface ICommunicator
    {
        /// <summary>
        /// Client side: Connects to the server and starts all threads.
        /// Server side: Starts the tcp client connect request listener
        /// and starts all threads.
        /// </summary>
        /// <param name="serverIP">
        /// IP Address of the server. Required only on client side.
        /// </param>
        /// <param name="serverPort">
        /// Port no. of the server. Required only on client side.
        /// </param>
        /// <returns>
        ///  Client side: string "success" if success, "failure" 
        ///  if failure
        /// Server side: If success then address of the server as a 
        ///  string of "IP:Port", else string "failure"
        /// </returns>
        public string Start(string serverIP = null, 
                string serverPort = null);

        /// <summary>
        /// Client side: Stops all threads, clears queues and 
        /// closes the socket.
        /// Server side: Stops listening to client connect requests 
        /// and stops all threads. And clears the queues.
        /// </summary>
        /// <returns> void </returns>
        public void Stop();

        /// <summary>
        /// This function is to be called by the Dashboard module on
        /// the server side when a new client joins. It adds the client
        /// socket to the map and starts listening to the client.
        /// </summary>
        /// <param name="clientId"> The client Id. </param>
        /// <param name="socket">
        /// The socket which is connected to the client.
        /// </param>
        /// <returns> void </returns>
        public void AddClient(string clientId, TcpClient socket);

        /// <summary>
        /// This function is to be called by the Dashboard module on
        /// the server side when a client leaves. It will remove the 
        /// client from the networking modules map on the server.
        /// </summary>
        /// <param name="clientId"> The client Id. </param>
        /// <returns> void </returns>
        public void RemoveClient(string clientId);

        /// <summary>
        /// Sends data from client to server or server to client(s).
        /// Client Side: Sends data to the server.
        /// Server Side: Sends data to a particular client if client
        /// id given in the destination argument, otherwise broadcasts
        /// data to all clients if destination null.
        /// </summary>
        /// <param name="serializedData">
        /// The serialzed data to be sent.
        /// </param>
        /// <param name="moduleOfPacket"> 
        /// Name of module sending the data.
        /// </param>
        /// <param name="destination">
        /// Client side: Not required on client side, give null.
        /// Server Side: Client Id of the client to which you want
        /// to send the data. To broadcast to all clients give null.
        /// </param>
        /// <returns> void </returns>
        public void Send(string serializedData, string moduleOfPacket,
                string? destination);

        /// <summary>
        /// Other modules can subscribe using this function to be able
        /// to send data. And be notified when data is received, and
        /// when a client joins, and when a client leaves.
        /// </summary>
        /// <param name="moduleName">  Name of the module. </param>
        /// <param name="notificationHandler">
        /// Module implementation of the INotificationHandler.
        /// </param>
        /// <param name="isHighPriority">
        /// Boolean telling whether module's data is high priority
        /// or low priority.
        /// </param>
        /// <returns> void </returns>
        public void Subscribe(string moduleName, INotificationHandler
                notificationHandler, bool isHighPriority = false);
    }

    public class Communicator : ICommunicator
    {
        private TcpListener? serverListener;
        private TcpClient? clientSocket;
        private readonly ConcurrentDictionary<string, TcpClient> connectedClients = new();
        private readonly ConcurrentDictionary<string, INotificationHandler> subscribedModules = new();
        private bool isServer = false;
        private CancellationTokenSource? cancellationTokenSource;

        public string Start(string serverIP = null, string serverPort = null)
        {
            if (serverIP == null && serverPort == null)
            {
                // Act as a server
                isServer = true;
                int port = 5000; // Default port for server
                serverListener = new TcpListener(IPAddress.Any, port);
                cancellationTokenSource = new CancellationTokenSource();

                // Start server listener in a new task
                Task.Run(() => ListenForClients(), cancellationTokenSource.Token);

                Console.WriteLine($"Server started on port {port}");
                return $"127.0.0.1:{port}";
            }
            else
            {
                // Act as a client
                try
                {
                    clientSocket = new TcpClient();
                    clientSocket.Connect(serverIP, int.Parse(serverPort));
                    Console.WriteLine("Connected to server");

                    // Start listening for server responses
                    Task.Run(() => ListenToServer(), cancellationTokenSource.Token);

                    return "success";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to server: {ex.Message}");
                    return "failure";
                }
            }
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            if (isServer)
            {
                serverListener?.Stop();
                Console.WriteLine("Server stopped");
            }
            else
            {
                clientSocket?.Close();
                Console.WriteLine("Client disconnected");
            }
        }

        public void AddClient(string clientId, TcpClient socket)
        {
            connectedClients.TryAdd(clientId, socket);
            NotifyClientJoined(socket);
        }

        public void RemoveClient(string clientId)
        {
            if (connectedClients.TryRemove(clientId, out TcpClient client))
            {
                client.Close();
                NotifyClientLeft(clientId);
            }
        }

        public void Send(string serializedData, string moduleOfPacket, string? destination)
        {
            byte[] data = Encoding.UTF8.GetBytes(serializedData);

            if (isServer)
            {
                if (destination == null)
                {
                    // Broadcast to all clients
                    foreach (var client in connectedClients.Values)
                    {
                        client.GetStream().Write(data, 0, data.Length);
                    }
                }
                else if (connectedClients.TryGetValue(destination, out TcpClient client))
                {
                    // Send to a specific client
                    client.GetStream().Write(data, 0, data.Length);
                }
            }
            else
            {
                // Send data to the server
                clientSocket?.GetStream().Write(data, 0, data.Length);
            }
        }

        public void Subscribe(string moduleName, INotificationHandler notificationHandler, bool isHighPriority = false)
        {
            subscribedModules.TryAdd(moduleName, notificationHandler);
        }

        private void ListenForClients()
        {
            serverListener?.Start();
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Accept new client
                    TcpClient clientSocket = serverListener.AcceptTcpClient();
                    string clientId = Guid.NewGuid().ToString();

                    // Add client and start listening for its messages
                    AddClient(clientId, clientSocket);
                    Task.Run(() => HandleClientCommunication(clientId, clientSocket), cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private void HandleClientCommunication(string clientId, TcpClient clientSocket)
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = clientSocket.GetStream();
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    int byteCount = stream.Read(buffer, 0, buffer.Length);
                    if (byteCount == 0) break; // Client disconnected

                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    // Notify subscribed modules about the received data
                    foreach (var handler in subscribedModules.Values)
                    {
                        handler.OnDataReceived(data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data from client: {ex.Message}");
                    break;
                }
            }

            RemoveClient(clientId);
        }

        private void ListenToServer()
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = clientSocket.GetStream();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    int byteCount = stream.Read(buffer, 0, buffer.Length);
                    if (byteCount == 0) break; // Server disconnected

                    string data = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    // Notify subscribed modules about the received data
                    foreach (var handler in subscribedModules.Values)
                    {
                        handler.OnDataReceived(data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving data from server: {ex.Message}");
                    break;
                }
            }

            Stop();
        }

        private void NotifyClientJoined(TcpClient clientSocket)
        {
            foreach (var handler in subscribedModules.Values)
            {
                handler.OnClientJoined(clientSocket);
            }
        }

        private void NotifyClientLeft(string clientId)
        {
            foreach (var handler in subscribedModules.Values)
            {
                handler.OnClientLeft(clientId);
            }
        }
    }
}
