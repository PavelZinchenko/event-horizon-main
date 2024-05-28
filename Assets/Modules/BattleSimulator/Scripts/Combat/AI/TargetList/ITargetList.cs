using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit;

namespace Combat.Ai
{
    public interface ITargetList
    {
        IReadOnlyList<IShip> Enemies { get; }
        IReadOnlyList<IShip> Allies { get; }
    }
}
