using System;
using System.IO;

namespace Session.Utils
{
	public class WriterStream : IWriterStream
	{
		private readonly Stream _stream;

		public WriterStream(Stream stream)
		{
			_stream = stream;
		}

		public void WriteByte(byte value) => _stream.WriteByte(value);
		public void Write(ReadOnlySpan<byte> buffer) => _stream.Write(buffer);
		public void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
	}

	public class MemoryWriterStream : IWriterStream
	{
		private readonly byte[] _buffer;
		private int _index;

		public int Position => _index;

		public MemoryWriterStream(byte[] buffer)
		{
			_buffer = buffer;
		}

		public void WriteByte(byte value) => _buffer[_index++] = value;
		
		public void Write(ReadOnlySpan<byte> buffer)
		{
			buffer.CopyTo(_buffer.AsSpan(_index));
			_index += buffer.Length;
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			Array.Copy(buffer, offset, _buffer, _index, count);
			_index += count;
		}
	}
}
