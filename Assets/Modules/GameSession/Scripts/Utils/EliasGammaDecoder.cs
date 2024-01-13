namespace Session.Utils
{
	public class EliasGammaDecoder
	{
		private readonly IReaderStream _reader;
		private readonly int _sectionSize;
		private byte _data;
		private int _length;

		public EliasGammaDecoder(IReaderStream reader, int sectionSize = 4)
		{
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
			var negative = ReadBit();
			var value = (long)ReadUnsigned();
			return negative ? -value : value;
		}

		public ulong ReadUnsigned()
		{
			int sections = 0;
			while (ReadBit()) sections++;
			if (sections == 0) return 0;
			var numberOfBits = sections * _sectionSize;

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
