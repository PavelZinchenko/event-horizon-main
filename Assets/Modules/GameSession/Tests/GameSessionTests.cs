using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Session.Utils;

namespace Session.Tests
{
	[TestFixture]
	public class GameSessionTests
	{
		private byte[] _buffer = new byte[65536];

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

		[Test]
		public void TestBitset()
		{
			var size = 100000;
			var keys = Enumerable.Range(0, size).Select(value => (uint)value).ToList();
			var random = new System.Random();
			for (int i = 0; i < keys.Count; ++i)
			{
				var index = random.Next(keys.Count);
				var temp = keys[i];
				keys[i] = keys[index];
				keys[index] = temp;
			}

			var bitset = new ObservableBitset(null);
			for (int i = 0; i < size / 2; ++i)
				bitset.Add(keys[i]);

			var stream = new MemoryWriterStream(_buffer);
			using (var writer = new SessionDataWriter(stream))
				bitset.Serialize(writer, EncodingType.EliasGamma);

			TestContext.WriteLine($"Size = {stream.Position} bytes");
			TestContext.WriteLine($"LastIndex = {bitset.LastIndex}");

			var reader = new SessionDataReader(new MemoryReaderStream(_buffer));
			bitset = new ObservableBitset(reader, EncodingType.EliasGamma, null);
			for (int i = 0; i < size; ++i)
			{
				var value = i < size / 2;
				if (bitset.Get(keys[i]) != value)
					Assert.AreEqual(bitset.Get(keys[i]), value);
			}
		}

        [Test]
        public void TestBitsetEnumeration()
        {
            var bitset1 = new ObservableBitset(null) { 1, 2, 5, 6, 7 };
            var bitset2 = new ObservableBitset(null) { 0, 1, 2, 5, 6, 7 };

            Assert.AreEqual(string.Join(' ', bitset1), "1 2 5 6 7");
            Assert.AreEqual(string.Join(' ', bitset2), "0 1 2 5 6 7");
        }

        [Test]
		public void TestSaveLoad()
		{
			const int quantity = 1000;
			int GetDefeatTime(int i) => i*i % 1000;
			int GetDefeatCount(int i) => i % 10; 

			var data1 = new Model.SaveGameData(null);
			for (int i = 0; i < quantity; ++i)
				data1.Bosses.Bosses.Add(i, new Model.BossInfo(GetDefeatCount(i), GetDefeatTime(i)));

			using (var writer = new SessionDataWriter(new MemoryWriterStream(_buffer)))
				data1.Serialize(writer);

			var reader = new SessionDataReader(new MemoryReaderStream(_buffer));
			var data2 = new Model.SaveGameData(reader, null);

			for (int i = 0; i < quantity; ++i)
			{
				Assert.AreEqual(GetDefeatCount(i), data2.Bosses.Bosses[i].DefeatCount);
				Assert.AreEqual(GetDefeatTime(i), data2.Bosses.Bosses[i].LastDefeatTime);
			}
		}

		[Test]
		public void TestShipSatellitesSaveLoad()
		{
			var data1 = new Model.SaveGameData(null);
			var fleet = data1.Fleet;
			var component = default(Model.ShipComponentInfo);
			fleet.Ships.Add(new Model.ShipInfo(1, "ship", 0, 0, new[] { component }, new byte[0], new long[0], new[]
			{
				new Model.SatelliteInfo(0, new Model.ShipComponentInfo[0], fleet),
				new Model.SatelliteInfo(7, new[] { component, component }, fleet),
				new Model.SatelliteInfo(9, new Model.ShipComponentInfo[0], fleet),
			}, fleet));
			var presets = data1.ShipPresets;
			presets.Presets.Add(new Model.ShipPresetInfo(2, "preset", new Model.ShipComponentInfo[0], new[]
			{
				new Model.SatelliteInfo(5, new[] { component }, presets),
			}, presets));

			using (var writer = new SessionDataWriter(new MemoryWriterStream(_buffer)))
				data1.Serialize(writer);

			var reader = new SessionDataReader(new MemoryReaderStream(_buffer));
			var data2 = new Model.SaveGameData(reader, null);

			var ship = data2.Fleet.Ships.Single();
			CollectionAssert.AreEqual(new[] { 0, 7, 9 }, ship.Satellites.Select(item => item.Id));
			Assert.AreEqual(2, ship.Satellites[1].Components.Count);
			var preset = data2.ShipPresets.Presets.Single();
			CollectionAssert.AreEqual(new[] { 5 }, preset.Satellites.Select(item => item.Id));
			Assert.AreEqual(1, preset.Satellites[0].Components.Count);
		}

		[TestCase(5, 6, new[] { 5, 6 })]
		[TestCase(5, 0, new[] { 5 })]
		[TestCase(0, 6, new[] { 0, 6 })]
		[TestCase(0, 0, new int[0])]
		public void TestShipSatellitesMigrationFromV3(int satellite1, int satellite2, int[] expected)
		{
			var oldData = new v3.SaveGameData(null);
			var component = default(Model.ShipComponentInfo);
			oldData.Fleet.Ships.Add(new v1.ShipInfo(1, "ship", 3, 4, new[] { component }, new byte[] { 1 }, new long[] { 2 },
				CreateSatellite(satellite1, oldData.Fleet), CreateSatellite(satellite2, oldData.Fleet), oldData.Fleet));
			oldData.ShipPresets.Presets.Add(CreateV3Preset(2, "preset", satellite1, satellite2));

			var convert = typeof(SessionLoader).GetMethod("Convert_3_4", BindingFlags.Instance | BindingFlags.NonPublic);
			var newData = (Model.SaveGameData)convert.Invoke(new SessionLoader(), new object[] { oldData });

			var ship = newData.Fleet.Ships.Single();
			Assert.AreEqual(1, ship.Id);
			Assert.AreEqual("ship", ship.Name);
			Assert.AreEqual(3, ship.ColorScheme);
			Assert.AreEqual(4, ship.Experience);
			Assert.AreEqual(1, ship.Components.Count);
			CollectionAssert.AreEqual(new byte[] { 1 }, ship.LayoutModifications);
			CollectionAssert.AreEqual(new long[] { 2 }, ship.Modifications);
			CollectionAssert.AreEqual(expected, ship.Satellites.Select(item => item.Id));
			foreach (var satellite in ship.Satellites)
				Assert.AreEqual(satellite.Id, satellite.Components.Count);

			var preset = newData.ShipPresets.Presets.Single();
			Assert.AreEqual(2, preset.Id);
			Assert.AreEqual("preset", preset.Name);
			CollectionAssert.AreEqual(expected, preset.Satellites.Select(item => item.Id));
			foreach (var satellite in preset.Satellites)
				Assert.AreEqual(satellite.Id, satellite.Components.Count);
		}

		[Test]
		public void TestShipDataSatelliteList()
		{
			var ship = CreateShipData(5, 0, 7);
			var data = ship.Serialize().ToArray();
			var index = 0;
			var result = ContentObsolete.ShipData.Deserialize(data, ref index);

			Assert.AreEqual(data.Length, index);
			CollectionAssert.AreEqual(new[] { 5, 0, 7 }, result.Satellites.Select(item => item.Id));
			CollectionAssert.AreEqual(new[] { 5, 0, 7 }, result.Satellites.Select(item => item.Components.Components.Count()));
		}

		[TestCase(new int[0], new[] { 0, 0 })]
		[TestCase(new[] { 5 }, new[] { 5, 0 })]
		[TestCase(new[] { 5, 6, 7 }, new[] { 5, 6 })]
		public void TestShipDataTwoSatellitesVersion(int[] satellites, int[] expected)
		{
			var data = CreateShipData(satellites).Serialize(ContentObsolete.ShipData.TwoSatellitesVersion).ToArray();
			var index = 0;
			var result = ContentObsolete.ShipData.Deserialize(data, ref index);

			Assert.AreEqual(ContentObsolete.ShipData.TwoSatellitesVersion, data[0]);
			Assert.AreEqual(data.Length, index);
			CollectionAssert.AreEqual(expected, result.Satellites.Select(item => item.Id));
		}

		[Test]
		public void TestShipDataInvalidSatelliteCount()
		{
			var data = CreateShipData().Serialize().ToArray();
			data[data.Length - 1] = 0x7f; // The last byte is the high byte of the satellite count.
			var index = 0;
			Assert.Throws<System.ArgumentException>(() => ContentObsolete.ShipData.Deserialize(data, ref index));
		}

		private static ContentObsolete.ShipData CreateShipData(params int[] satellites)
		{
			return new ContentObsolete.ShipData
			{
				Id = 11,
				Name = "ship",
				Satellites = satellites.Select(id => new ContentObsolete.SatelliteData
				{
					Id = id,
					Components = new ContentObsolete.ShipComponentsData
					{
						Components = Enumerable.Repeat(new ContentObsolete.ShipComponentsData.Component { Id = id }, id),
					},
				}).ToArray(),
			};
		}

		// A satellite with id N has N components, so the tests can check that components move with their satellite.
		private static Model.SatelliteInfo CreateSatellite(int id, IDataChangedCallback callback)
		{
			return new Model.SatelliteInfo(id, Enumerable.Repeat(default(Model.ShipComponentInfo), id), callback);
		}

		private v3.ShipPresetInfo CreateV3Preset(int id, string name, int satellite1, int satellite2)
		{
			using (var writer = new SessionDataWriter(new MemoryWriterStream(_buffer)))
			{
				writer.WriteInt(id, EncodingType.EliasGamma);
				writer.WriteString(name, EncodingType.EliasGamma);
				writer.WriteInt(0, EncodingType.EliasGamma);
				CreateSatellite(satellite1, null).Serialize(writer);
				CreateSatellite(satellite2, null).Serialize(writer);
			}

			return new v3.ShipPresetInfo(new SessionDataReader(new MemoryReaderStream(_buffer)), null);
		}
	}
}
