using Shared.Enums;
using SimpleTCP.Models;
using Shared.Models.Messages;
using SimpleTCP.Server;
using System;
using System.Threading;

namespace ConsoleClient
{
    class Program
    {
        //thread safe list
        private static AsyncClient<CommandType> _server;

        public static void Main(string[] args)
        {
            _server = new AsyncClient<CommandType>();
            _server.OnRemoteSocketDisconnected += RemoteSocketDisconnected;
            _server.Start("127.0.0.1");

            Console.WriteLine("\nPress ENTER to continue...");

            while(true)
            {
                _server.Send(new ChatMessage("Client hellooo"));
                Thread.Sleep(new TimeSpan(0, 0, 5));
            }
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
