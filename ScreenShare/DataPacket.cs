// Defines the class "DataPacket" which represents the data packet sent
// from server to client or the other way.

using System.Text.Json.Serialization;

namespace ScreenShare
{
  
    // Represents the data packet sent from server to client or the other way.
  
    public class DataPacket
    {
        
        // Creates an instance of the DataPacket with empty string values for all
        
        public DataPacket()
        {
            Id = "";
            Name = "";
            Header = "";
            Data = "";
        }

        
        // Creates an instance of the DataPacket containing the header field
        // and data field in the packet used for communication between server
        
        [JsonConstructor]
        public DataPacket(string id, string name, string header, string data)
        {
            Id = id;
            Name = name;
            Header = header;
            Data = data;
        }

       
        // Gets the id field of the packet.
       
        public string Id { get; private set; }

        // Gets the name field of the packet.

        public string Name { get; private set; }

        // Gets the header field of the packet.
        // Possible headers from the server: Send, Stop
        // Possible headers from the client: Register, Deregister, Image, Confirmation
    
        public string Header { get; private set; }
        
        // Gets the data field of the packet.
        // Data corresponding to various headers:
        
        public string Data { get; private set; }
    }
}