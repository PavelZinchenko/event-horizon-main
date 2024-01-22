using System.Collections.Generic;
using Combat.Component.Platform;
using Combat.Component.Ship;

namespace Combat.Component.Systems
{
    public class ShipSystems : IShipSystems
    {
        public ShipSystems(IShip ship)
        {
            _ship = ship;
			_systems = new();
			_systemsReadonly = _systems.AsReadOnly();
        }

        public IReadOnlyList<ISystem> All => _systemsReadonly;

        public SystemsModifications Modifications { get { return _modifications; } }

        public void UpdatePhysics(float elapsedTime)
        {
            foreach (var platform in _weaponPlatforms)
                platform.UpdatePhysics(elapsedTime);

            _modifications.Cleanup();

            var count = _systems.Count;
            for (var index = 0; index < count; ++index)
            {
                var system = _systems[index];

                if (ShouldActivateSystem(index, system))
                {
                    system.Active = true;
                    _modifications.OnSystemActivated(system);
                }
                else
                {
                    system.Active = false;
                }

                system.UpdatePhysics(elapsedTime);
            }
        }

        public void UpdateView(float elapsedTime)
        {
            foreach (var platform in _weaponPlatforms)
                platform.UpdateView(elapsedTime);

            var count = _systems.Count;
            for (var index = 0; index < count; ++index)
            {
                var system = _systems[index];
                system.UpdateView(elapsedTime);
            }
        }

        public void OnEvent(SystemEventType eventType)
        {
            var count = _systems.Count;
            for (var index = 0; index < count; ++index)
            {
                var system = _systems[index];
                system.OnEvent(eventType);
            }
        }

        public void Add(ISystem system)
        {
            _systems.Add(system);
        }

        public void Add(IWeaponPlatform platform)
        {
            _weaponPlatforms.Add(platform);
        }

        public void Dispose()
        {
            foreach (var system in _systems)
                system.Dispose();

            foreach (var platform in _weaponPlatforms)
                platform.Dispose();
        }

        private bool ShouldActivateSystem(int id, ISystem system)
        {
            if (!system.CanBeActivated)
                return false;

            var keyBinding = system.KeyBinding;
            if (keyBinding < 0)
            {
                var energyLeft = _ship.Stats.Energy.Percentage + 0.1f * (keyBinding + 1);
                if (system.Active)
                    energyLeft += system.ActivationCost/_ship.Stats.Energy.MaxValue;
                if (energyLeft < 0)
                    return false;
            }
            else if (!_ship.Controls.Systems[id])
                return false;

            return _modifications.CanActivateSystem(system);
        }

        private readonly IShip _ship;
		private readonly List<ISystem> _systems;
		private readonly IReadOnlyList<ISystem> _systemsReadonly;
		private readonly List<IWeaponPlatform> _weaponPlatforms = new List<IWeaponPlatform>();
        private readonly SystemsModifications _modifications = new SystemsModifications();
    }
}
