using Shared.Models.Messages;
using Shared.Enums;
using SimpleTCP.Models;
using SimpleTCP.Server;
using System;
using System.Threading;

namespace ConsoleServer
{
    class Program
    {
        //thread safe list
        private static AsyncServer<CommandType> _server;

        public static void Main(string[] args)
        {
            _server = new AsyncServer<CommandType>();
            _server.OnRemoteSocketConnected += RemoteSocketConnected;
            _server.OnRemoteSocketDisconnected += RemoteSocketDisconnected;
            _server.Start();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void RemoteSocketConnected(NetConnection connection)
        {
        }

        private static void RemoteSocketDisconnected(NetConnection connection)
        {
        }

        private static void OnChatMessageReceived(ChatMessage message, NetConnection connection)
        {
            //process the received ips
            try
            {
                Console.WriteLine(message.Text);

                Thread.Sleep(new TimeSpan(0, 0, 2));
                _server.Send(connection, new ChatMessage("Server hellooo client"));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
