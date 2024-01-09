using System.Collections.Generic;
using System.Linq;
using CommonComponents.Utils;
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

		public DatabaseEditorContext(IDatabase database, IShip ship)
		{
			_database = database;
			Ship = ship;
			Inventory = new DatabaseInventoryProvider(database);
		}

		public IShip Ship { get; }
		public IInventoryProvider Inventory { get; }
		public bool IsTechResearched(Component component)
		{
			var technology = _database.TechnologyList.FirstOrDefault(item => item is Technology_Component tech && tech.Component == component);
			return technology != null;
		}

		private class DatabaseInventoryProvider : IInventoryProvider
		{
			private readonly GameItemCollection<Satellite> _satellites = new();
			private readonly GameItemCollection<ComponentInfo> _components;
			private readonly List<ISatellite> _satelliteBuilds;
			private readonly List<IShip> _ships;

			public IReadOnlyGameItemCollection<ComponentInfo> Components => _components;
			public IReadOnlyGameItemCollection<Satellite> Satellites => _satellites;
			public IReadOnlyCollection<ISatellite> SatelliteBuilds => _satelliteBuilds;
			public IEnumerable<IShip> Ships => _ships;

			public DatabaseInventoryProvider(IDatabase database)
			{
				_components = new();
				foreach (var item in database.ComponentList)
				{
					var common = new ComponentInfo(item);
					_components.Add(common, 999);
					foreach (var mod in item.PossibleModifications)
					{
						var component = new ComponentInfo(item, mod.Type, GameDatabase.Enums.ModificationQuality.P3);
						_components.Add(component, 999);
					}
				}

				_ships = database.ShipBuildList.Select<ShipBuild, IShip>(build => new EditorModeShip(build, database)).ToList();
				_satelliteBuilds = database.SatelliteBuildList.Select<SatelliteBuild, ISatellite>(build => new EditorModeSatellite(build, database)).ToList();
			}

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
				return component.Price * 2;
			}

			public bool TryPayForUnlock(ComponentInfo component)
			{
				//UnityEngine.Debug.LogError($"TryPayForUnlock - {GetUnlockPrice(component)}");
				return true;
			}
		}
	}
}
