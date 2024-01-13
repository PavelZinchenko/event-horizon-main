namespace Session.Utils
{
	public class EliasGammaDecoder
	{
		private readonly IReaderStream _reader;
		private readonly uint _sectionSize;
		private byte _data;
		private int _length;

		public EliasGammaDecoder(IReaderStream reader, uint sectionSize = 4)
		{
			if (sectionSize == 0) 
				throw new System.ArgumentException();

			_reader = reader;
			_sectionSize = sectionSize;
		}

		public void Flush()
		{
			_data = 0;
			_length = 0;
		}

		public bool ReadBool() => ReadBit();

		public long ReadSigned()
		{
			var sections = ReadUnary();
			var numberOfBits = sections * _sectionSize;
			var negative = ReadBit();
			var value = (long)ReadBinary(numberOfBits);
			return negative ? -value : value;
		}

		public ulong ReadUnsigned()
		{
			var sections = ReadUnary();
			var numberOfBits = sections * _sectionSize;
			return ReadBinary(numberOfBits);
		}

		private uint ReadUnary()
		{
			uint value = 0;
			while (ReadBit()) value++;
			return value;
		}

		private ulong ReadBinary(uint numberOfBits)
		{
			ulong value = 0;
			for (int i = 0; i < numberOfBits; ++i)
				if (ReadBit())
					value |= 1ul << i;

			return value;
		}

		private bool ReadBit()
		{
			if (_length == 0)
			{
				_data = _reader.ReadByte();
				_length = 8;
			}

			var value = _data & 1;
			_data >>= 1;
			_length--;

			return value != 0;
		}
	}
}
