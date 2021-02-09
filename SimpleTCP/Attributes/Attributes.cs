using System;

namespace SimpleTCP.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CommandTypeAttribute : Attribute
    {
        public byte Command { get; private set; }

        public CommandTypeAttribute(byte command)
        {
            Command = command;
        }
    }
}
