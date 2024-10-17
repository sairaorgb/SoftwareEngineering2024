 
// Defines the interface "ITimerManager" which will be implemented by the
// server model which will contain the timeout callback for screen sharing.
 

using System.Timers;

namespace ScreenShare.Server
{
     
    // Interface to be implemented by the server model which will contain the
    // timeout callback for screen sharing.
     
    public interface ITimerManager
    {
         
        // Callback which will be invoked when the timeout occurs for the
        // CONFIRMATION packet not received by the client.

        public void OnTimeOut(object? source, ElapsedEventArgs e, string id);
    }
}
