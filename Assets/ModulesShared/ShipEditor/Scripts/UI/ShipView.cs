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
			_elements[ShipElementType.SatelliteR].Width +
			_satelliteOffset*2;

		public float Height => Mathf.Max(Mathf.Max(
			_elements[ShipElementType.SatelliteL].Height, 
			_elements[ShipElementType.SatelliteR].Height), 
			_elements[ShipElementType.Ship].Height);

        public Vector2 Position { get => transform.localPosition; set => transform.localPosition = value; }
        public float Rotation { get => transform.localEulerAngles.z; set => transform.localEulerAngles = new Vector3(0, 0, value); }
        public float Scale { get => transform.localScale.z; set => transform.localScale = value * Vector3.one; }
        public float CellSize => _cellSize * transform.localScale.z;

		public void InitializeShip(IShipLayoutModel layout, Sprite sprite)
		{
			_elements[ShipElementType.Ship].Initialize(layout, sprite, _cellSize);
			UpdateSatellitePosition(SatelliteLocation.Left);
			UpdateSatellitePosition(SatelliteLocation.Right);
		}

		public void RemoveSatellite(SatelliteLocation location) => InitializeSatellite(location, null, null);
		public void InitializeSatellite(SatelliteLocation location, IShipLayoutModel layout, Sprite sprite, float imageScaleMultiplier = 1f)
		{
			_elements[location].Initialize(layout, sprite, _cellSize, imageScaleMultiplier);
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
            center = transform.InverseTransformPoint(center.x, center.y, 0);
			var offset = view.ContentOffset + view.transform.localPosition;
			var x = (center.x - offset.x) / _cellSize - 0.5f * componentSize;
			var y = (offset.y - center.y) / _cellSize - 0.5f * componentSize;
			return new Vector2Int(Mathf.RoundToInt(x + view.X0), Mathf.RoundToInt(y + view.Y0));
		}

		public void ShowSelection(Vector2 position, DraggableComponent.Content item)
		{
			var size = item.Layout.Size;
			var component = item.Component.Data;
			_elements[ShipElementType.Ship].ShowSelection(WorldToCell(position, ShipElementType.Ship, size), component, item.Layout);
			_elements[ShipElementType.SatelliteL].ShowSelection(WorldToCell(position, ShipElementType.SatelliteL, size), component, item.Layout);
			_elements[ShipElementType.SatelliteR].ShowSelection(WorldToCell(position, ShipElementType.SatelliteR, size), component, item.Layout);
		}

		public void ClearSelection()
		{
			var empty = new GameDatabase.Model.Layout(string.Empty);
			_elements[ShipElementType.Ship].ShowSelection(Vector2Int.zero, null, empty);
			_elements[ShipElementType.SatelliteL].ShowSelection(Vector2Int.zero, null, empty);
			_elements[ShipElementType.SatelliteR].ShowSelection(Vector2Int.zero, null, empty);
		}

		public void AddComponent(IComponentModel component)
		{
			_elements[component.Location].AddComponent(component);
		}

		public void RemoveComponent(IComponentModel component)
		{
			_elements[component.Location].RemoveComponent(component);
		}

		public void ReloadAllComponents(ShipElementType shipElement)
		{
			_elements[shipElement].GenerateModules();
			_elements[shipElement].GenerateWeaponClasses();
		}

		public void UpdateComponent(IComponentModel component)
		{
			_elements[component.Location].UpdateComponent(component);
		}
	}
}
