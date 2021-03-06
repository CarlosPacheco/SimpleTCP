using SimpleTCP.Interfaces;
using SimpleTCP.Models;
using System;
using System.Collections.Generic;

namespace SimpleTCP.Core
{
    public class MessageDescriptor<TMessage, TCommandType> : IMessageDescriptor<TCommandType>
        where TMessage : class
        where TCommandType : struct, Enum
    {
        readonly List<Action<TMessage, NetConnection>> _callbacks = new List<Action<TMessage, NetConnection>>();

        void IMessageDescriptor<TCommandType>.RegisterCallback(Delegate callback)
        {
            _callbacks.Add((Action<TMessage, NetConnection>)callback);
        }

        void IMessageDescriptor<TCommandType>.Invoke(IMessage<TCommandType> msg, NetConnection socket)
        {
            foreach (Action<TMessage, NetConnection> callback in _callbacks)
            {
                callback?.Invoke((TMessage)msg, socket);
            }
        }
    }
}
