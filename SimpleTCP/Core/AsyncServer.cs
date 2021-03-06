using SimpleTCP.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleTCP.Core
{
    public class AsyncServer<TCommandType> : AsyncPeer<TCommandType>, IAsyncServer<TCommandType>
       where TCommandType : struct, Enum
    {
        public Action<NetConnection> OnRemoteSocketConnected { get; set; }

        // Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);

        public AsyncServer(bool enableAutoRegisterCallback = true) : base(enableAutoRegisterCallback)
        {
        }

        public override void Start(string ip = "", int port = 10500)
        {
            // Create a TCP/IP socket.
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                socket.Bind(new IPEndPoint(string.IsNullOrWhiteSpace(ip) ? IPAddress.Any : IPAddress.Parse(ip), port));
                socket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            Console.WriteLine("new connection....");
            // Get the socket that handles the client request.
            Socket socket = ((Socket)ar.AsyncState).EndAccept(ar);

            // Create the state object.
            NetConnection connection = new NetConnection(BufferSize, socket);
            OnRemoteSocketConnected?.Invoke(connection);

            socket.BeginReceive(connection.buffer, 0, BufferSize, 0, new AsyncCallback(ReceiveCallback), connection);
        }
    }
}
