using SimpleTCP.Interfaces;
using SimpleTCP.Models;
using System;
using System.Net.Sockets;

namespace SimpleTCP.Core
{
    public class AsyncClient<TCommandType> : AsyncPeer<TCommandType>, IAsyncClient<TCommandType>
       where TCommandType : struct, Enum
    {
        // Create the state object.
        private NetConnection _connection;

        public AsyncClient(bool enableAutoRegisterCallback = true) : base(enableAutoRegisterCallback)
        {
        }

        public override void Start(string ip, int port = 10500)
        {
            // Create a TCP/IP socket.
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                socket.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), socket);

                Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
                // Release the socket.  
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                // Release the socket.  
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket socket = (Socket)ar.AsyncState;
            _connection = new NetConnection(BufferSize, socket);
            socket.BeginReceive(_connection.buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), _connection);
        }

        public void Send(IMessage<TCommandType> message) => Send(_connection, message);
    }
}
