using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using Constructor.Ships;
using Constructor.Satellites;
using Constructor;
using Economy;

namespace ShipEditor.Context
{
	public class DatabaseEditorContext : IShipEditorContext
	{
		private readonly IDatabase _database;

		public DatabaseEditorContext(IDatabase database, IShip ship, [Zenject.InjectOptional] IShipPresetStorage shipPresetStorage = null)
		{
			_database = database;
			Ship = ship;
			Inventory = new DatabaseInventoryProvider(database);
            ShipDataProvider = new EmptyDataProvider();
            ShipPresetStorage = shipPresetStorage ?? new EmptyShipPresetStorage();
            UpgradesProvider = new EmptyUpgradesProvider();
		}

		public IShip Ship { get; }
		public IInventoryProvider Inventory { get; }
        public IShipDataProvider ShipDataProvider { get; }
        public bool IsShipNameEditable => false;
        public IShipPresetStorage ShipPresetStorage { get; }
        public IComponentUpgradesProvider UpgradesProvider { get; }

        public bool CanBeUnlocked(Component component)
		{
			var technology = _database.TechnologyList.FirstOrDefault(item => item is Technology_Component tech && tech.Component == component);
			return technology != null;
		}

		private class DatabaseInventoryProvider : IInventoryProvider
		{
			private readonly List<Satellite> _satellites = new();
			private readonly List<ComponentInfo> _components = new();
			private readonly List<ISatellite> _satelliteBuilds;
			private readonly List<IShip> _ships;

			public IReadOnlyCollection<ComponentInfo> Components => _components;
			public IReadOnlyCollection<Satellite> Satellites => _satellites;
			public IReadOnlyCollection<ISatellite> SatelliteBuilds => _satelliteBuilds;
			public IEnumerable<IShip> Ships => _ships;

			public DatabaseInventoryProvider(IDatabase database)
			{
				foreach (var item in database.ComponentList)
				{
					var common = new ComponentInfo(item);
					_components.Add(common);
					foreach (var mod in item.PossibleModifications)
					{
						var component = new ComponentInfo(item, mod, GameDatabase.Enums.ModificationQuality.P3);
						_components.Add(component);
					}
				}

				_ships = database.ShipBuildList.Select<ShipBuild, IShip>(build => new EditorModeShip(build, database)).ToList();
				_satelliteBuilds = database.SatelliteBuildList.Select<SatelliteBuild, ISatellite>(build => new EditorModeSatellite(build, database)).ToList();
			}

            public int GetQuantity(Satellite satellite) => 999;
            public int GetQuantity(ComponentInfo component) => 999;

            public void AddComponent(ComponentInfo component)
			{
				//UnityEngine.Debug.LogError($"AddComopnent {component.Data.Name}");
			}

			public bool TryRemoveComponent(ComponentInfo component)
			{
				//UnityEngine.Debug.LogError($"RemoveComopnent {component.Data.Name}");
				return true;
			}

			public void AddSatellite(Satellite satellite)
			{
				//UnityEngine.Debug.LogError($"AddSatellite {satellite.Name}");
			}

			public bool TryRemoveSatellite(Satellite satellite)
			{
				//UnityEngine.Debug.LogError($"RemoveSatellite {satellite.Name}");
				return true;
			}

			public Price GetUnlockPrice(ComponentInfo component)
			{
				return Price.Common(100);
			}

			public bool TryPayForUnlock(ComponentInfo component)
			{
				//UnityEngine.Debug.LogError($"TryPayForUnlock - {GetUnlockPrice(component)}");
				return true;
			}
        }

        private class EmptyShipPresetStorage : IShipPresetStorage
        {
            private List<IShipPreset> _presets = new();

            public IShipPreset Create(Ship ship)
            {
                var preset = new ShipPreset(ship);
                _presets.Add(preset);
                return preset;
            }

            public void Update(IShipPreset preset)
            {
                // In-memory storage is already updated by reference.
            }

            public void Delete(IShipPreset preset)
            {
                _presets.Remove(preset);
            }

            public IEnumerable<IShipPreset> GetPresets(Ship ship)
            {
                return _presets.Where(item => item.Ship == ship);
            }
        }

        private class EmptyUpgradesProvider : IComponentUpgradesProvider
        {
            public IEnumerable<ComponentUpgradeLevel> GetAllUpgrades() => Enumerable.Empty<ComponentUpgradeLevel>();
            public IComponentUpgrades GetComponentUpgrades(Component component) => null;
        }
    }
}
