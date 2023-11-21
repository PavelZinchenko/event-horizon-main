using System;
using System.IO;

namespace CommonComponents.Serialization
{
    public static class StreamExtensions
    {
        public static int ReadInt32(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            stream.Read(buffer);
            return BitConverter.ToInt32(buffer);
        }

        public static uint ReadUInt32(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            stream.Read(buffer);
            return BitConverter.ToUInt32(buffer);
        }

        public static string ReadString(this Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            stream.Read(buffer);
            var size = BitConverter.ToInt32(buffer);
            if (size == 0) return string.Empty;

            if (size < 0xff)
                buffer = stackalloc byte[size];
            else
                buffer = new byte[size];

            var totalRead = stream.Read(buffer);
            if (totalRead < size) return string.Empty;

            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        public static byte[] ReadByteArray(this Stream stream)
        {
            var length = stream.ReadInt32();
            if (length <= 0)
                return Array.Empty<byte>();

            var array = new byte[length];
            var totalRead = stream.Read(array, 0, length);
            if (totalRead < length)
                return Array.Empty<byte>();

            return array;
        }
    }
}
