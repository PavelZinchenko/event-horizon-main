using System;

namespace Session.Utils
{
	public enum EncodingType
	{
		Plain,
		EliasGamma,
	}

	public class SessionDataWriter : IDisposable
	{
		private readonly IWriterStream _writer;
		private readonly EliasGammaEncoder _eliasEncoder;
		private EncodingType _encoding;

		public SessionDataWriter(IWriterStream writer)
		{
			_writer = writer;
			_eliasEncoder = new EliasGammaEncoder(writer);
		}

		public void Dispose()
		{
			_eliasEncoder.Dispose();
		}

		public void WriteBool(bool value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteBool(value);
			else
				_writer.WriteBool(value);
		}

		public void WriteByte(byte value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteUnsigned(value);
			else
				_writer.WriteByte(value);
		}

		public void WriteSbyte(sbyte value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteSigned(value);
			else
				_writer.WriteSbyte(value);
		}

		public void WriteShort(short value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteSigned(value);
			else
				_writer.WriteShort(value);
		}

		public void WriteUshort(ushort value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteUnsigned(value);
			else
				_writer.WriteUshort(value);
		}

		public void WriteInt(int value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteSigned(value);
			else
				_writer.WriteInt(value);
		}

		public void WriteUint(uint value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteUnsigned(value);
			else
				_writer.WriteUint(value);
		}

		public void WriteLong(long value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteSigned(value);
			else
				_writer.WriteLong(value);
		}

		public void WriteUlong(ulong value, EncodingType encoding)
		{
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				_eliasEncoder.WriteUnsigned(value);
			else
				_writer.WriteUlong(value);
		}

		public void WriteFloat(float value, EncodingType encoding)
		{
			encoding = EncodingType.Plain;
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				;// TODO: _encoder.WriteFloat(value);
			else
				_writer.WriteFloat(value);
		}

		public void WriteString(string value, EncodingType encoding)
		{
			encoding = EncodingType.Plain;
			UpdateEncoding(encoding);
			if (encoding == EncodingType.EliasGamma)
				;// TODO: _encoder.WriteString(value);
			else
				_writer.WriteString(value);
		}

		private void UpdateEncoding(EncodingType encoding)
		{
			if (_encoding == encoding) return;

			if (_encoding == EncodingType.EliasGamma)
				_eliasEncoder.Flush();

			_encoding = encoding;
		}
	}
}
