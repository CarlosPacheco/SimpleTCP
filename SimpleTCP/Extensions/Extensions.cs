using System.IO;
using System.Text;

namespace SimpleTCP.Extensions
{
    public static class Extensions
    {
        public static void WriteStringUtf8(this BinaryWriter stream, string value)
        {
            stream.Write(Encoding.UTF8.GetByteCount(value));
            stream.Write(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] ToArray(this BinaryWriter stream)
        {
            return ((MemoryStream)stream.BaseStream).ToArray();
        }

        public static string ReadStringUtf8(this BinaryReader stream)
        {
            return Encoding.UTF8.GetString(stream.ReadBytes(stream.ReadInt32()));
        }

    }
}
