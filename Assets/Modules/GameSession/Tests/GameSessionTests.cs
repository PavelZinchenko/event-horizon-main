using NUnit.Framework;
using Session.Utils;

namespace Session.Tests
{
	[TestFixture]
	public class GameSessionTests
	{
		private byte[] _buffer = new byte[256];

		[Test]
		public void TestStreamReader()
		{
			var writer = new MemoryWriterStream(_buffer);
			var reader = new MemoryReaderStream(_buffer);

			writer.WriteLong(12345);
			writer.WriteInt(67890);
			Assert.AreEqual(12345, reader.ReadLong());
			Assert.AreEqual(67890, reader.ReadInt());
		}

		[Test]
		public void TestEliasGammaEncoder()
		{
			var writer = new MemoryWriterStream(_buffer);
			var reader = new MemoryReaderStream(_buffer);
			
			using (var encoder = new EliasGammaEncoder(writer))
			{
				encoder.WriteBool(true);
				encoder.WriteSigned(-12345);
				encoder.WriteUnsigned(0);
				encoder.WriteUnsigned(12345);
				encoder.WriteSigned(1);
				encoder.WriteSigned(1234567890);
			}

			var decoder = new EliasGammaDecoder(reader);
			Assert.AreEqual(true, decoder.ReadBool());
			Assert.AreEqual(-12345, decoder.ReadSigned());
			Assert.AreEqual(0, decoder.ReadUnsigned());
			Assert.AreEqual(12345, decoder.ReadUnsigned());
			Assert.AreEqual(1, decoder.ReadSigned());
			Assert.AreEqual(1234567890, decoder.ReadSigned());
		}

		[Test]
		public void TestSessionDataWriter()
		{
			using (var writer = new SessionDataWriter(new MemoryWriterStream(_buffer)))
			{
				writer.WriteBool(true, EncodingType.EliasGamma);
				writer.WriteInt(-12345, EncodingType.EliasGamma);
				writer.WriteUlong(0, EncodingType.Plain);
				writer.WriteUint(12345, EncodingType.Plain);
				writer.WriteSbyte(1, EncodingType.EliasGamma);
				writer.WriteLong(1234567890L, EncodingType.EliasGamma);
			}

			var reader = new SessionDataReader(new MemoryReaderStream(_buffer));

			Assert.AreEqual(true, reader.ReadBool(EncodingType.EliasGamma));
			Assert.AreEqual(-12345, reader.ReadInt(EncodingType.EliasGamma));
			Assert.AreEqual(0, reader.ReadUlong(EncodingType.Plain));
			Assert.AreEqual(12345, reader.ReadUint(EncodingType.Plain));
			Assert.AreEqual(1, reader.ReadSbyte(EncodingType.EliasGamma));
			Assert.AreEqual(1234567890L, reader.ReadLong(EncodingType.EliasGamma));
		}
	}
}
