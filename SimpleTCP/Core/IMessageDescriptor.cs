using SimpleTCP.Interfaces;
using SimpleTCP.Models;
using System;

namespace SimpleTCP.Core
{
    public interface IMessageDescriptor<T> where T : struct, Enum
    {
        void RegisterCallback(Delegate callback);

        void Invoke(IMessage<T> msg, NetConnection socket);
    }
}
