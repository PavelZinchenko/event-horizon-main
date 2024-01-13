using System;

namespace Session.Utils
{
	public interface IReaderStream
	{
		byte ReadByte();
		int Read(Span<byte> buffer);
		int Read(byte[] buffer, int offset, int count);
	}

	public static class ReaderStreamExtensions
	{
		public static bool ReadBool(this IReaderStream reader) => reader.ReadByte() != 0;
		public static sbyte ReadSbyte(this IReaderStream reader) => (sbyte)reader.ReadByte();
		
		public static short ReadShort(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(short)];
			reader.Read(buffer);
			return BitConverter.ToInt16(buffer);
		}

		public static ushort ReadUshort(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(ushort)];
			reader.Read(buffer);
			return BitConverter.ToUInt16(buffer);
		}

		public static int ReadInt(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(int)];
			reader.Read(buffer);
			return BitConverter.ToInt32(buffer);
		}

		public static uint ReadUint(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(uint)];
			reader.Read(buffer);
			return BitConverter.ToUInt32(buffer);
		}

		public static long ReadLong(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(long)];
			reader.Read(buffer);
			return BitConverter.ToInt64(buffer);
		}

		public static ulong ReadUlong(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(ulong)];
			reader.Read(buffer);
			return BitConverter.ToUInt64(buffer);
		}

		public static float ReadFloat(this IReaderStream reader)
		{
			Span<byte> buffer = stackalloc byte[sizeof(float)];
			reader.Read(buffer);
			return BitConverter.ToSingle(buffer);
		}

		public static string ReadString(this IReaderStream reader)
		{
			var length = ReadInt(reader);
			if (length == 0) return string.Empty;

			var data = new byte[length];
			reader.Read(data, 0, length);
			return System.Text.Encoding.UTF8.GetString(data);
		}
	}
}
