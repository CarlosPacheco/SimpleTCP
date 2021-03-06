using SimpleTCP.Interfaces;
using SimpleTCP.Models;
using System;

namespace SimpleTCP.Core
{
    public interface IAsyncClient<TCommandType> where TCommandType : struct, Enum
    {
        Action<NetConnection> OnRemoteSocketDisconnected { get; set; }

        void AddMessageHandler(TCommandType key, Tuple<Type, IMessageDescriptor<TCommandType>> value);    

        /// <summary>
        /// Register a global action for one message type.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="callback">callback action</param>
        /// <param name="commandType">the command type</param>
        void RegisterCallbackTo<TEvent>(Action<TEvent, NetConnection> callback, TCommandType commandType) where TEvent : class, IMessage<TCommandType>;

        void Start(string ip, int port = 10500);

        void Send(IMessage<TCommandType> message);
    }
}
