namespace Communicator;
using System.Net.Sockets;

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


}

