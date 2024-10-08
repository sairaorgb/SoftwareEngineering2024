namespace Communicator;
using System.Net.Sockets;
public interface IServerCommunicator : ICommunicator
{
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

