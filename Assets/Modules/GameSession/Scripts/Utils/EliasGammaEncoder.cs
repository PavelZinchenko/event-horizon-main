using System;

namespace Session.Utils
{
	public class EliasGammaEncoder : IDisposable
	{
		private readonly IWriterStream _writer;
		private readonly int _sectionSize;
		private byte _data;
		private int _index;

		public EliasGammaEncoder(IWriterStream writer, int sectionSize = 4)
		{
			_writer = writer;
			_sectionSize = sectionSize;
		}

		public void Flush()
		{
			if (_index > 0)
			{
				_writer.WriteByte(_data);
				_index = 0;
				_data = 0;
			}
		}

		public void WriteBool(bool value) => WriteBit(value);

		public void WriteSigned(long value)
		{
			WriteBit(value < 0);
			WriteUnsigned(value < 0 ? (ulong)(-value) : (ulong)value);
		}

		public void WriteUnsigned(ulong value)
		{
			if (value == 0)
			{
				WriteBit(false);
				return;
			}

			var sections = (NumberOfBits(value) + _sectionSize - 1) / _sectionSize;
			var numberOfBits = sections * _sectionSize;

			for (int i = 0; i < sections; ++i) WriteBit(true);
			WriteBit(false);

			ulong oldValue = value;
			for (int i = 0; i < numberOfBits; ++i)
			{
				WriteBit((value & 1) != 0);
				value >>= 1;
			}
		}

		public void Dispose() => Flush();

		private void WriteBit(bool value)
		{
			if (value) _data = (byte)(_data | (1 << _index));
			_index++;

			if (_index == 8)
			{
				_writer.WriteByte(_data);
				_index = 0;
				_data = 0;
			}
		}

		private static int NumberOfBits(ulong value)
		{
			int index = 0;
			while (value != 0)
			{
				value >>= 1;
				index++;
			}

			return index;
		}
	}
}
