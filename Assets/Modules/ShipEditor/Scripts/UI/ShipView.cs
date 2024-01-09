using UnityEngine;
using ShipEditor.Model;

namespace ShipEditor.UI
{
	public class ShipView : MonoBehaviour
	{
		[SerializeField] private float _satelliteOffset = 2;
		[SerializeField] private float _cellSize = 1.0f;
		[SerializeField] private ShipElementContainer<EditorShipLayout> _elements;

		public float Width => 
			_elements[ShipElementType.Ship].Width + 
			_elements[ShipElementType.SatelliteL].Width + 
			_elements[ShipElementType.SatelliteR].Width;

		public float Height => Mathf.Max(Mathf.Max(
			_elements[ShipElementType.SatelliteL].Height, 
			_elements[ShipElementType.SatelliteR].Height), 
			_elements[ShipElementType.Ship].Height);

		public void InitializeShip(IShipLayoutModel layout, Sprite sprite)
		{
			_elements[ShipElementType.Ship].Initialize(layout, sprite, _cellSize);
			UpdateSatellitePosition(SatelliteLocation.Left);
			UpdateSatellitePosition(SatelliteLocation.Right);
		}

		public void RemoveSatellite(SatelliteLocation location) => InitializeSatellite(location, null, null);
		public void InitializeSatellite(SatelliteLocation location, IShipLayoutModel layout, Sprite sprite)
		{
			_elements[location].Initialize(layout, sprite, _cellSize);
			UpdateSatellitePosition(location);
		}

		private void UpdateSatellitePosition(SatelliteLocation location)
		{
			var shipView = _elements[ShipElementType.Ship];
			var satelliteView = _elements[location];
			var offset = (shipView.Width + satelliteView.Width) / 2 + _satelliteOffset;
			if (location == SatelliteLocation.Left) offset = -offset;
			var shipPosition = shipView.transform.localPosition;
			satelliteView.transform.localPosition = new Vector3(shipPosition.x + offset, shipPosition.y, shipPosition.z);
		}

		public Vector2Int WorldToCell(Vector2 center, ShipElementType element, int componentSize)
		{
			var view = _elements[element];
			var offset = transform.localPosition + view.ContentOffset + view.transform.localPosition;
			var x = Mathf.RoundToInt((center.x - offset.x) / _cellSize - 0.5f * componentSize);
			var y = Mathf.RoundToInt((offset.y - center.y) / _cellSize - 0.5f * componentSize);
			return new Vector2Int(x, y);
		}

		public void ShowSelection(Vector2 position, GameDatabase.DataModel.Component component)
		{
			var size = component != null ? component.Layout.Size : 0;
			_elements[ShipElementType.Ship].ShowSelection(WorldToCell(position, ShipElementType.Ship, size), component);
			_elements[ShipElementType.SatelliteL].ShowSelection(WorldToCell(position, ShipElementType.SatelliteL, size), component);
			_elements[ShipElementType.SatelliteR].ShowSelection(WorldToCell(position, ShipElementType.SatelliteR, size), component);
		}

		public void AddComponent(IComponentModel component, ShipElementType shipElement)
		{
			_elements[shipElement].AddComponent(component);
		}

		public void RemoveComponent(IComponentModel component, ShipElementType shipElement)
		{
			_elements[shipElement].RemoveComponent(component);
		}

		public void ReloadAllComponents(ShipElementType shipElement)
		{
			_elements[shipElement].GenerateModules();
			_elements[shipElement].GenerateWeaponClasses();
		}

		public void UpdateComponent(IComponentModel model, ShipElementType shipElement)
		{
			_elements[shipElement].UpdateComponent(model);
		}
	}
}
