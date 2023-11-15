using System.Collections.Generic;

namespace Services.Storage
{
    public interface IGameData
    {
        long GameId { get; }
        long TimePlayed { get; }
        long DataVersion { get; }
        string ModId { get; }
    }
}
