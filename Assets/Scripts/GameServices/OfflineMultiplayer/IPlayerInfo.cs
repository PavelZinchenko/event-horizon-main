using System.Collections.Generic;
using Constructor.Ships;
using Cysharp.Threading.Tasks;

namespace GameServices.Multiplayer
{
    public interface IPlayerInfo
    {
        int Id { get; }
        string Name { get; }
        int Rating { get; }

        UniTask<bool> LoadFleet();
        IEnumerable<IShip> Fleet { get; }
        float PowerMultiplier { get; }
    }
}
