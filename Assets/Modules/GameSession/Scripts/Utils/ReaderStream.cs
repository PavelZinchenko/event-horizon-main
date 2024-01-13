using System.IO;
using System;

namespace Session.Utils
{
	public class ReaderStream : IReaderStream
	{
		private readonly Stream _stream;

		public ReaderStream(Stream stream)
		{
			_stream = stream;
		}

		public byte ReadByte() => (byte)_stream.ReadByte();
		public int Read(Span<byte> buffer) => _stream.Read(buffer);
		public int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
	}

	public class MemoryReaderStream : IReaderStream
	{
		private readonly byte[] _data;
		private int _index = 0;

		public MemoryReaderStream(byte[] data, int startIndex = 0)
		{
			_data = data;
			_index = startIndex;
		}

		public byte ReadByte() => _data[_index++];
		public int Read(Span<byte> buffer) 
		{
			var length = buffer.Length;
			_data.AsSpan(_index, length).CopyTo(buffer);
			_index += length;
			return length;
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			Array.Copy(_data, _index, buffer, offset, count);
			_index += count;
			return count;
		}
	}
}
