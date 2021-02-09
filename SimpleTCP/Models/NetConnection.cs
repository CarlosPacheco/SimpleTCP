using System.IO;
using System.Net.Sockets;

namespace SimpleTCP.Models
{
    /// <summary>
    /// State object for reading client data asynchronously
    /// from: https://msdn.microsoft.com/en-us/library/fx6588te(v=vs.110).aspx
    /// </summary>
    public class NetConnection
    {
        // Client  socket.
        public Socket Socket { get; set; }

        // Receive buffer.
        public byte[] buffer;

        public NetConnection(int bufferSize, Socket socket)
        {
            buffer = new byte[bufferSize];
            Socket = socket;
        }

        public object Tag { get; set; }

        /// <summary>
        /// Stream copy from the buffer property
        /// </summary>
        public MemoryStream Stream => new MemoryStream(buffer);
    }
}
