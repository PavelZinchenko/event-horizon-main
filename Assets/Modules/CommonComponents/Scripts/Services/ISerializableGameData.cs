using System.Collections.Generic;

namespace Services.Storage
{
    public interface ISerializableGameData : IGameData
    {
        IEnumerable<byte> Serialize();
        bool TryDeserialize(long gameId, long timePlayed, long dataVersion, string modId, byte[] data, int startIndex);
    }
}
