using System;

namespace Session.Utils
{
	public class EliasGammaEncoder : IDisposable
	{
		private readonly IWriterStream _writer;
		private readonly uint _sectionSize;
		private byte _data;
		private int _index;

		public EliasGammaEncoder(IWriterStream writer, uint sectionSize = 4)
		{
			if (sectionSize == 0)
				throw new ArgumentException();

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
			if (value == 0) { WriteBit(false); return; }

			ulong unsignedValue = value > 0 ? (ulong)value : (ulong)(-value);
			uint sections = (NumberOfBits(unsignedValue) + _sectionSize - 1) / _sectionSize;
			uint numberOfBits = sections * _sectionSize;

			WriteUnary(sections);
			WriteBit(value < 0);
			WriteBinary(unsignedValue, numberOfBits);
		}

		public void WriteUnsigned(ulong value)
		{
			if (value == 0) { WriteBit(false); return; }

			uint sections = (NumberOfBits(value) + _sectionSize - 1) / _sectionSize;
			uint numberOfBits = sections * _sectionSize;
			WriteUnary(sections);
			WriteBinary(value, numberOfBits);
		}

		private void WriteUnary(uint numberOfBits)
		{
			for (uint i = 0; i < numberOfBits; ++i) WriteBit(true);
			WriteBit(false);
		}

		private void WriteBinary(ulong value, uint numberOfBits)
		{
			for (uint i = 0; i < numberOfBits; ++i)
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

		private static uint NumberOfBits(ulong value)
		{
			uint index = 0;
			while (value != 0)
			{
				value >>= 1;
				index++;
			}

			return index;
		}
	}
}
