using System;
using System.Linq;
using System.Collections.Generic;
using ShipEditor.Context;
using Constructor;
using Constructor.Ships;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace ShipEditor.Model
{
	public enum SatelliteLocation
	{
		Left,
		Right,
	}

	public enum ShipElementType
	{
		Ship,
		SatelliteL,
		SatelliteR,
	}
	
	public interface IShipEditorModel
	{
		IShipEditorEvents Events { get; }

		IShip Ship { get; }
		IInventoryProvider Inventory { get; }
		ICompatibilityChecker CompatibilityChecker { get; }
		IEnumerable<IComponentModel> InstalledComponents { get; }
		string ShipName { get; set; }

		IShipLayoutModel Layout(SatelliteLocation location);
		IShipLayoutModel Layout(ShipElementType elementType);

		Satellite Satellite(SatelliteLocation location);
		bool HasSatellite(SatelliteLocation location);
		void RemoveSatellite(SatelliteLocation location);
		void InstallSatellite(SatelliteLocation location, Satellite satellite);

		void SelectShip(IShip ship);

		bool TryFindComponent(IComponentModel component, out ShipElementType shipElement);
		bool TryFindComponent(UnityEngine.Vector2Int position, ComponentInfo info, out IComponentModel component, out ShipElementType shipElement);
		bool TryInstallComponent(UnityEngine.Vector2Int position, ShipElementType shipElement, ComponentInfo componentInfo, ComponentSettings settings);
		void RemoveComponent(ShipElementType shipElement, IComponentModel component);
		void RemoveAllComponents();

		void SetComponentKeyBinding(IComponentModel component, int key);
		void SetComponentBehaviour(IComponentModel component, int behaviour);

		bool CanBeUnlocked(IComponentModel component);
		void UnlockComponent(IComponentModel component);
	}

	public class ShipEditorModel : IShipEditorModel, IDisposable
	{
		private readonly ShipEditorEvents _events = new();
		private readonly IShipEditorContext _context;
		private ShipElementContainer<ShipLayoutModel> _layout;
		private ShipElementContainer<Satellite> _satellite;
		private ComponentTracker _compatibilityChecker;
		private IShip _ship;
		private string _shipName;

		public IShipEditorEvents Events => _events;

		public IShip Ship => _ship;
		public IInventoryProvider Inventory => _context.Inventory;
		public ICompatibilityChecker CompatibilityChecker => _compatibilityChecker;
		
		public IEnumerable<IComponentModel> InstalledComponents
		{
			get 
			{
				var components = _layout[ShipElementType.Ship].Components;
				if (_layout[ShipElementType.SatelliteL] != null)
					components = components.Concat(_layout[ShipElementType.SatelliteL].Components);
				if (_layout[ShipElementType.SatelliteR] != null)
					components = components.Concat(_layout[ShipElementType.SatelliteR].Components);

				return components;
			}
		}

		public IShipLayoutModel Layout(SatelliteLocation location) => _layout[location];
		public IShipLayoutModel Layout(ShipElementType elementType) => _layout[elementType];
		public bool HasSatellite(SatelliteLocation location) => _layout[location] != null;
		public Satellite Satellite(SatelliteLocation location) => _satellite[location];

		public string ShipName 
		{
			get => _shipName ?? _ship?.Name;
			set => _shipName = value;
		}

		public ShipEditorModel(IShipEditorContext context)
		{
			_context = context;
			SelectShip(_context.Ship);
		}

		public void SelectShip(IShip ship)
		{
			SaveShip(_ship);

			_ship = ship;
			_compatibilityChecker = new ComponentTracker(ship);
			_shipName = null;

			var shipLayout = _layout[ShipElementType.Ship] = new ShipLayoutModel(ship.Model.Layout, ship.Model.Barrels, _compatibilityChecker);
			InitializeLayout(shipLayout, ship.Components);
			InitializeSatellite(SatelliteLocation.Left, _ship.FirstSatellite?.Information, _ship.FirstSatellite?.Components);
			InitializeSatellite(SatelliteLocation.Right, _ship.SecondSatellite?.Information, _ship.SecondSatellite?.Components);

			_events.OnShipChanged(ship);
		}

		public void InstallSatellite(SatelliteLocation location, Satellite satellite)
		{
			if (_satellite[location] == satellite) return;
			RemoveSatellite(location);

			if (satellite == null) return;

			if (!_compatibilityChecker.IsCompatible(satellite))
				throw new InvalidOperationException();

			if (!_context.Inventory.TryRemoveSatellite(satellite))
				throw new InvalidOperationException();

			InitializeSatellite(location, satellite, null);
		}

		public void RemoveSatellite(SatelliteLocation location)
		{
			if (_layout[location] == null) return;

			RemoveAllComopnents(_layout[location]);
			_events.OnMultipleComponentsChanged();

			_layout[location] = null;

			if (_satellite[location] != null)
			{
				_context.Inventory.AddSatellite(_satellite[location]);
				_satellite[location] = null;
			}

			_events.OnSatelliteChanged(location);
		}

		public void SetComponentKeyBinding(IComponentModel component, int key)
		{
			if (key != component.KeyBinding)
				TryUpdateComponent(component, new ComponentSettings(key, component.Behaviour, component.Locked));
		}

		public void SetComponentBehaviour(IComponentModel component, int behaviour)
		{
			if (behaviour != component.Behaviour)
				TryUpdateComponent(component, new ComponentSettings(component.KeyBinding, behaviour, component.Locked));
		}


		public bool CanBeUnlocked(IComponentModel component)
		{
			if (!component.Locked)
				return false;
			if (component.Info.Data.Availability == Availability.Common)
				return true;

			return _context.IsTechResearched(component.Data);
		}

		public void UnlockComponent(IComponentModel component)
		{
			if (!CanBeUnlocked(component))
				throw new InvalidOperationException();

			if (!_context.Inventory.TryPayForUnlock(component.Info))
				throw new InvalidOperationException();

			TryUpdateComponent(component, new ComponentSettings(component.KeyBinding, component.Behaviour, false));
		}

		public bool TryInstallComponent(UnityEngine.Vector2Int position, ShipElementType shipElement, ComponentInfo componentInfo, ComponentSettings settings)
		{
			var layout = _layout[shipElement];
			if (layout == null)
				return false;

			if (!_compatibilityChecker.IsCompatible(componentInfo.Data))
				return false;

			if (!layout.IsSuitableLocation(position.x, position.y, componentInfo.Data))
				return false;

			if (!_context.Inventory.TryRemoveComponent(componentInfo))
				return false;

			var component = layout.InstallComponent(position.x, position.y, componentInfo, settings);
			_events.OnComponentAdded(component, shipElement);
			return true;
		}

		public void RemoveComponent(ShipElementType elementType, IComponentModel component)
		{
			if (component.Locked)
				throw new InvalidOperationException();

			_layout[elementType].RemoveComponent(component);
			_context.Inventory.AddComponent(component.Info);
			_events.OnComponentRemoved(component, elementType);
		}

		public void RemoveAllComponents()
		{
			RemoveAllComopnents(_layout[ShipElementType.Ship]);
			RemoveAllComopnents(_layout[ShipElementType.SatelliteL]);
			RemoveAllComopnents(_layout[ShipElementType.SatelliteR]);
			_events.OnMultipleComponentsChanged();
		}

		public void Dispose()
		{
			SaveShip(_ship);
		}

		public bool TryFindComponent(IComponentModel component, out ShipElementType elementType)
		{
			elementType = ShipElementType.Ship;
			if (_layout[elementType].HasComponent(component))
				return true;

			elementType = ShipElementType.SatelliteL;
			if (_layout[elementType].HasComponent(component))
				return true;

			elementType = ShipElementType.SatelliteR;
			if (_layout[elementType].HasComponent(component))
				return true;

			return false;
		}

		public bool TryFindComponent(UnityEngine.Vector2Int position, ComponentInfo info, out IComponentModel component, out ShipElementType elementType)
		{
			elementType = ShipElementType.Ship;
			component = _layout[ShipElementType.Ship].FindComponent(position.x, position.y, info);
			if (component != null) return true;

			elementType = ShipElementType.SatelliteL;
			component = _layout[ShipElementType.Ship].FindComponent(position.x, position.y, info);
			if (component != null) return true;

			elementType = ShipElementType.SatelliteR;
			component = _layout[ShipElementType.Ship].FindComponent(position.x, position.y, info);
			return component != null;
		}

		private void SaveShip(IShip ship)
		{
			if (ship == null) return;

			Ship.Components.Assign(ExportComponents(_layout[ShipElementType.Ship]));

			if (!string.IsNullOrEmpty(_shipName))
				Ship.Name = _shipName;

			_ship.FirstSatellite = _satellite[SatelliteLocation.Left] == null ? null :
				new Constructor.Satellites.CommonSatellite(_satellite[SatelliteLocation.Left], ExportComponents(_layout[SatelliteLocation.Left]));
			_ship.SecondSatellite = _satellite[SatelliteLocation.Right] == null ? null :
				new Constructor.Satellites.CommonSatellite(_satellite[SatelliteLocation.Right], ExportComponents(_layout[SatelliteLocation.Right]));
		}

		private static IEnumerable<IntegratedComponent> ExportComponents(ShipLayoutModel layout)
		{
			if (layout == null) yield break;
			foreach (var model in layout.Components)
				yield return new IntegratedComponent(model.Info, model.X, model.Y, 
					layout.GetBarrelId(model), model.KeyBinding, model.Behaviour, model.Locked);
		}

		private void RemoveAllComopnents(ShipLayoutModel layout, bool keepLocked = true)
		{
			if (layout == null) return;

			foreach (var item in layout.Components)
				if (!keepLocked || !item.Locked)
					_context.Inventory.AddComponent(item.Info);

			layout.RemoveAll(keepLocked);
		}

		private bool TryUpdateComponent(IComponentModel component, ComponentSettings settings)
		{
			if (!TryFindComponent(component, out var elementType))
				return false;

			_layout[elementType].UpdateComponent(component, settings);
			_events.OnComponentModified(component, elementType);

			return true;
		}

		private void InitializeLayout(ShipLayoutModel layout, IEnumerable<IntegratedComponent> components)
		{
			if (components == null) return;

			foreach (var component in components)
			{
				if (!layout.IsSuitableLocation(component.X, component.Y, component.Info.Data))
				{
					UnityEngine.Debug.LogError($"Invalid component {component.Info.Data.Name} at [{component.X},{component.Y}]");
					continue;
				}

				layout.InstallComponent(component.X, component.Y, component.Info,
					new ComponentSettings(component.KeyBinding, component.Behaviour, component.Locked));
			}
		}

		private void InitializeSatellite(SatelliteLocation location, Satellite satellite, IEnumerable<IntegratedComponent> components)
		{
			_satellite[location] = satellite;
			if (satellite != null)
			{
				_layout[location] = new ShipLayoutModel(satellite.Layout, satellite.Barrels, _compatibilityChecker);
				InitializeLayout(_layout[location], components);
			}
			else
			{
				_layout[location] = null;
			}

			_events.OnSatelliteChanged(location);
		}
	}
}
