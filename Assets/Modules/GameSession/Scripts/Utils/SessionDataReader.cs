namespace Session.Utils
{
	public class SessionDataReader
	{
		private readonly IReaderStream _reader;
		private readonly EliasGammaDecoder _eliasDecoder;
		private EncodingType _encoding;

		public SessionDataReader(IReaderStream reader)
		{
			_reader = reader;
			_eliasDecoder = new EliasGammaDecoder(reader);
		}

		public bool ReadBool(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return _eliasDecoder.ReadBool();

			return _reader.ReadBool();
		}

		public byte ReadByte(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (byte)_eliasDecoder.ReadUnsigned();

			return _reader.ReadByte();
		}

		public sbyte ReadSbyte(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (sbyte)_eliasDecoder.ReadSigned();

			return _reader.ReadSbyte();
		}

		public short ReadShort(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (short)_eliasDecoder.ReadSigned();

			return _reader.ReadShort();
		}

		public ushort ReadUshort(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (ushort)_eliasDecoder.ReadUnsigned();

			return _reader.ReadUshort();
		}

		public int ReadInt(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (int)_eliasDecoder.ReadSigned();

			return _reader.ReadInt();
		}

		public uint ReadUint(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return (uint)_eliasDecoder.ReadUnsigned();

			return _reader.ReadUint();
		}

		public long ReadLong(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return _eliasDecoder.ReadSigned();

			return _reader.ReadLong();
		}

		public ulong ReadUlong(EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				return _eliasDecoder.ReadUnsigned();

			return _reader.ReadUlong();
		}

		public float ReadFloat(EncodingType encoding)
		{
			encoding = EncodingType.Plain;
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				;// TODO: return _decoder.ReadUnsigned();

			return _reader.ReadFloat();
		}

		public string ReadString(EncodingType encoding)
		{
			encoding = EncodingType.Plain;
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				;// TODO: return _decoder.ReadString();

			return _reader.ReadString();
		}

		private void UpdateEncoding(EncodingType encoding)
		{
			if (_encoding == encoding) return;
			_encoding = encoding;

			if (_encoding == EncodingType.EliasGamma)
				_eliasDecoder.Flush();
		}
	}
}
