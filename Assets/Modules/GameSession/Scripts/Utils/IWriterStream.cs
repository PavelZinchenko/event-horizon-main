using System;

namespace Session.Utils
{
	public interface IWriterStream
	{
		void WriteByte(byte value);
		void Write(ReadOnlySpan<byte> buffer);
		void Write(byte[] buffer, int offset, int count);
	}

	public static class WriterStreamExtensions
	{
		public static void WriteBool(this IWriterStream writer, bool value) => writer.WriteByte(value ? (byte)1 : (byte)0);
		public static void WriteSbyte(this IWriterStream writer, sbyte value) => writer.WriteByte((byte)value);

		public static void WriteShort(this IWriterStream writer, short value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(short)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteUshort(this IWriterStream writer, ushort value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(ushort)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteInt(this IWriterStream writer, int value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(int)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteUint(this IWriterStream writer, uint value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(uint)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteLong(this IWriterStream writer, long value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(long)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteUlong(this IWriterStream writer, ulong value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(ulong)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteFloat(this IWriterStream writer, float value)
		{
			Span<byte> buffer = stackalloc byte[sizeof(float)];
			BitConverter.TryWriteBytes(buffer, value);
			writer.Write(buffer);
		}

		public static void WriteString(this IWriterStream writer, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				writer.WriteInt(0);
				return;
			}

			var bytes = System.Text.Encoding.UTF8.GetBytes(value);
			writer.WriteInt(value.Length);
			writer.Write(bytes, 0, bytes.Length);
		}
	}
}
