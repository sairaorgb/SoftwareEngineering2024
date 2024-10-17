 
// Defines the interface "IMessageListener" which will be implemented by
// the server view model and used by the server data model to notify view model.
 

using ScreenShare.ScreenShareServer;
using System.Collections.Generic;

namespace ScreenShare.Server
{
     
    // Interface to be implemented by the server view model and used by
    // the server data model to notify the view model.
     
    public interface IMessageListener
    {
         
        // Notifies that subscribers list has been changed.
        // This will happen when a client either starts or stops screen sharing.
        public void OnSubscribersChanged(List<SharedClientScreen> subscribers);

         
        // Notifies that a client has started screen sharing.
        public void OnScreenshareStart(string clientId, string clientName);

         
        // Notifies that a client has stopped screen sharing.
        public void OnScreenshareStop(string clientId, string clientName);
    }
}
