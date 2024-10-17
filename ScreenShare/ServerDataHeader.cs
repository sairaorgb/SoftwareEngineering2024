// Defines the enum "ServerDataHeader", which enumerates all the headers
// that could be present in the data packet sent by the server.

using System.Runtime.Serialization;

namespace ScreenShare
{
    // Enumerates all the headers that could be present in the data packet
    // sent by the server.
    public enum ServerDataHeader
    {
        // Ask/Tell the client to start sending Image data packets
        // with the given resolution.
        [EnumMember(Value = "SEND")]
        Send,

        // Ask the client to stop sending Image data packets.
        [EnumMember(Value = "STOP")]
        Stop,

        // Confirmation packet sent to the client.
        [EnumMember(Value = "CONFIRMATION")]
        Confirmation
    }
}