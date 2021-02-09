using Shared.Enums;
using SimpleTCP.Interfaces;
using System.IO;
using SimpleTCP.Extensions;

namespace Shared.Models.Messages
{
    public class ChatMessage : IMessage<CommandType>
    {
        /// <summary>
        /// Unique IMessage Id
        /// </summary>
        public CommandType Id => CommandType.Chat;

        /// <summary>
        /// Text message
        /// </summary>
        public string Text;

        public ChatMessage()
        {
        }

        public ChatMessage(string text)
        {
            Text = text;
        }

        public ChatMessage(BinaryReader binReader)
        {
            OnDeserialize(binReader);
        }

        public void OnSerialize(BinaryWriter stream)
        {
            stream.Write((byte)Id);
            stream.WriteStringUtf8(Text);
        }

        public void OnDeserialize(BinaryReader stream)
        {
            //read the int from the string lenght and the string
            Text = stream.ReadStringUtf8();
        }
    }
}

