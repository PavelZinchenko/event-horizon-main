using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Constructor.Model;

namespace Combat.Ai.BehaviorTree
{
    public class ShipCapabilities
    {
        private readonly IShip _ship;
        private readonly GameDatabase.Enums.AiDifficultyLevel _aiLevel;
        private readonly List<WeaponWrapper> _weapons = new();

        public GameDatabase.Enums.AiDifficultyLevel AiLevel => _aiLevel;
        public IReadOnlyList<WeaponWrapper> Weapons => _weapons;
        public IShipStats Stats => _ship.Specification.Stats;
        public IShip Ship => _ship;

        public ShipCapabilities(IShip ship, GameDatabase.Enums.AiDifficultyLevel aiDifficultyLevel)
        {
            _ship = ship;
            _aiLevel = aiDifficultyLevel;

            for (int i = 0; i < _ship.Systems.All.Count; ++i)
            {
                var system = _ship.Systems.All[i];
                if (system is IWeapon weapon)
                    _weapons.Add(new WeaponWrapper(i, weapon, this));
            }
        }
    }
}
