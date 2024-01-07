using System;
using Constructor.Ships;

namespace ShipEditor.Model
{
	public interface IShipEditorEventTriggers
	{
		void OnComponentAdded(IComponentModel component, ShipElementType shipElement);
		void OnComponentRemoved(IComponentModel component, ShipElementType shipElement);
		void OnComponentModified(IComponentModel component, ShipElementType shipElement);
		void OnShipChanged(IShip ship);
		void OnSatelliteChanged(SatelliteLocation location);
		void OnMultipleComponentsChanged();
	}

	public interface IShipEditorEvents
	{
		event Action<IShip> ShipChanged;
		event Action<SatelliteLocation> SatelliteChanged;
		event Action<IComponentModel, ShipElementType> ComponentAdded;
		event Action<IComponentModel, ShipElementType> ComponentRemoved;
		event Action<IComponentModel, ShipElementType> ComponentModified;
		event Action MultipleComponentsChanged;
	}

	public class ShipEditorEvents : IShipEditorEvents, IShipEditorEventTriggers
	{
		public event Action<IShip> ShipChanged;
		public event Action<SatelliteLocation> SatelliteChanged;
		public event Action<IComponentModel, ShipElementType> ComponentAdded;
		public event Action<IComponentModel, ShipElementType> ComponentRemoved;
		public event Action<IComponentModel, ShipElementType> ComponentModified;
		public event Action MultipleComponentsChanged;

		public void OnComponentAdded(IComponentModel component, ShipElementType shipElement) => ComponentAdded?.Invoke(component, shipElement);
		public void OnComponentRemoved(IComponentModel component, ShipElementType shipElement) => ComponentRemoved?.Invoke(component, shipElement);
		public void OnComponentModified(IComponentModel component, ShipElementType shipElement) => ComponentModified?.Invoke(component, shipElement);
		public void OnShipChanged(IShip ship) => ShipChanged?.Invoke(ship);
		public void OnSatelliteChanged(SatelliteLocation location) => SatelliteChanged?.Invoke(location);
		public void OnMultipleComponentsChanged() => MultipleComponentsChanged?.Invoke();
	}
}
