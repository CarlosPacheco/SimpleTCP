using System.IO;

namespace SimpleTCP.Interfaces
{
    public interface IMessage<T> where T : struct
    {
        /// <summary>
        /// Unique IMessage Id
        /// </summary>
        T Id { get; }

        void OnSerialize(BinaryWriter stream);

        void OnDeserialize(BinaryReader stream);
    }
}
