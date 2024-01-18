using System;
using Session.Utils;
using Session.Model;

namespace Session
{
	public class SessionSerializer
	{
		private const uint _header = 0x5AFE5AFE;
		private const uint _eofMarker = 0xDEADC0DE;
		private const int _formatMin = 1;
		private const int _format = 2;

		private readonly ContentFactoryObsolete _contentFactoryObsolete;

		public SessionSerializer(ContentFactoryObsolete contentFactoryObsolete)
		{
			_contentFactoryObsolete = contentFactoryObsolete;
		}

		public bool TryDeserialize(byte[] data, int startIndex, out SaveGameData content)
		{
			try
			{
				var reader = new MemoryReaderStream(data, startIndex);
				var header = reader.ReadUint();
				if (header != _header)
					return new LegacyDataLoader(_contentFactoryObsolete).TryDeserializeOldData(data, startIndex, out content);

				var format = reader.ReadInt();
				return TryDeserializeContent(reader, format, out content);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				content = null;
				return false;
			}
		}

		public void Serialize(SaveGameData data, IWriterStream writer)
		{
			writer.WriteUint(_header);
			writer.WriteInt(_format);
			var version = SaveGameData.VersionMajor << 16 + SaveGameData.VersionMinor;
			writer.WriteInt(version);

			using (var sessionDataWriter = new SessionDataWriter(writer))
				data.Serialize(sessionDataWriter);

			writer.WriteUint(_eofMarker);
		}

		public byte[] Serialize(SaveGameData data)
		{
			using (var stream = new System.IO.MemoryStream())
			{
				Serialize(data, new WriterStream(stream));
				return stream.ToArray();
			}
		}

		private bool TryDeserializeContent(IReaderStream reader, int format, out SaveGameData content)
		{
			if (format < _formatMin || format > _format)
			{
				GameDiagnostics.Trace.LogError($"{nameof(SessionSerializer)}: savegame format is not supported - {format}");
				content = null;
				return false;
			}

			bool checkEof = format > 1;

			var version = reader.ReadInt();
			var versionMinor = version & 0xffff;
			var versionMajor = version >> 16;

			content = new SessionLoader().Load(new SessionDataReader(reader), versionMajor, versionMinor);

			if (checkEof && reader.ReadUint() != _eofMarker)
			{
				GameDiagnostics.Trace.LogError($"{nameof(SessionSerializer)}: EoF marker was not found");
				return false;
			}

			return true;
		}
	}
}
