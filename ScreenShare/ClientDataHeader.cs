// Defines the enum "ClientDataHeader", which enumerates all the headers
// that could be present in the data packet sent by the client.

using System.Runtime.Serialization;

namespace ScreenShare
{

    // Enumerates all the headers that could be present in the data packet
    // sent by the client.
    
    public enum ClientDataHeader
    {
         
        // Register a client for screen sharing.
      
        [EnumMember(Value = "REGISTER")]
        Register,

       
        // De-register a client for screen sharing.
       
        [EnumMember(Value = "DEREGISTER")]
        Deregister,

        
        // Image received from the client.
      
        [EnumMember(Value = "IMAGE")]
        Image,

      
        // Confirmation packet received from the client.
      
        [EnumMember(Value = "CONFIRMATION")]
        Confirmation
    }
}