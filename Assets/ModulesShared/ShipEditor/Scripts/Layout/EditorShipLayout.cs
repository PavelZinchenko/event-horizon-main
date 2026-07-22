using UnityEngine;
using GameDatabase.Enums;
using GameDatabase.Model;
using Constructor.Model;

namespace ShipEditor
{
	public class EditorShipLayout : MonoBehaviour
	{
		[SerializeField] private Color _innerCellColor;
		[SerializeField] private Color _outerCellColor;
		[SerializeField] private Color _engineCellColor;
		[SerializeField] private Color _weaponCellColor;
		[SerializeField] private Color _validCellColor;
		[SerializeField] private Color _invalidCellColor;
		[SerializeField] private Color _lockedCellColor;

		[SerializeField] private Transform _content;
		[SerializeField] private ShipLayoutElement _body;
		[SerializeField] private ShipModulesLayout _modules;
		[SerializeField] private ShipLayoutElement _selection;
		[SerializeField] private ShipLayoutElement _lockedCells;
		[SerializeField] private WeaponClassLayout _weaponClasses;
		[SerializeField] private SpriteRenderer _shipImage;

		[SerializeField] private float _lockSize = 0.5f;

		private float _cellSize;

        public float X0 => _shipLayout == null ? 0 : _shipLayout.Rect.xMin;
        public float Y0 => _shipLayout == null ? 0 : _shipLayout.Rect.yMin;
        public float Width => _shipLayout == null ? 0 : _shipLayout.Rect.Width * _cellSize;
		public float Height => _shipLayout == null ? 0 : _shipLayout.Rect.Height * _cellSize;
        public int OriginalSize => _shipLayout.OriginalSize;

        public Vector3 ContentOffset => _content.localPosition;

		private LockedCellsMeshBuilder _lockedCellBuilder;
		private Model.IShipLayoutModel _shipLayout;
		private Vector2Int _selectedPosition;

		public void Initialize(Model.IShipLayoutModel layout, Sprite sprite, float cellSize, float imageScaleMultiplier = 1f)
		{
			_cellSize = cellSize;
			_shipLayout = layout;
			_selection.SetMesh(null);

			GenerateMesh();
			GenerateModules();
			GenerateWeaponClasses();

			_shipImage.gameObject.SetActive(sprite != null);
			if (sprite != null)
			{
				_shipImage.sprite = sprite;
                var size = _shipLayout == null ? 0 : _shipLayout.OriginalSize;
                var offsetX = _shipLayout == null ? 0 : (0.5f*size - _shipLayout.Rect.xMin) * _cellSize;
                var offsetY = _shipLayout == null ? 0 : (0.5f*size - _shipLayout.Rect.yMin) * _cellSize;

                var imagePosition = new Vector3(offsetX, -offsetY, _shipImage.transform.localPosition.z);
                // Every ship sprite now uses the database grid's native 1.0 scale.
                // Per-name multipliers caused both visual/grid mismatch and editor
                // pointer-offset reports when switching between ship variants.
                var imageScale = size * _cellSize * Mathf.Max(0.01f, imageScaleMultiplier);
				// Keep the source aspect ratio.  The circular hull sections are the
				// calibration reference: any per-axis correction turns them into an
				// ellipse and makes the artwork look stretched against the grid.
                _shipImage.transform.localPosition = imagePosition;
				_shipImage.transform.localScale = imageScale * Vector3.one;
			}

			_content.localPosition = new Vector3(-Width / 2, Height / 2, 0);
		}

		public void GenerateModules()
		{
			if (_shipLayout == null)
            {
                _modules.Initialize(_cellSize,0,0);
                return;
            }

            _modules.Initialize(_cellSize, _shipLayout.Rect.xMin, _shipLayout.Rect.yMin);
			foreach (var item in _shipLayout.Components)
				_modules.AddComponent(item, false);

			_modules.UpdateMesh();

			GenerateLockedCells();
		}

		public void UpdateComponent(Model.IComponentModel component)
		{
			// A rotation changes the occupied-cell outline as well as the image.
			// Rebuilding keeps locked overlays and weapon-slot labels in sync.
			GenerateModules();
			GenerateWeaponClasses();
		}

		public void ShowSelection(Vector2Int position, GameDatabase.DataModel.Component component, Layout layout)
		{
			if (component == null || _shipLayout == null)
			{
				_selection.SetMesh(null);
				return;
			}

			if (position == _selectedPosition) return;
			_selectedPosition = position;

			var cellValidator = new CellValidator(_shipLayout, component);
			var builder = new SelectionMeshBuilder(cellValidator, _cellSize, _shipLayout.Rect.xMin, _shipLayout.Rect.yMin);
			builder.ValidCellColor = _validCellColor;
			builder.InvalidCellColor = _invalidCellColor;
			builder.Build(layout, position.x, position.y);
			_selection.SetMesh(builder.CreateMesh());
		}

		public void AddComponent(Model.IComponentModel component)
		{
			_modules.AddComponent(component);
			_weaponClasses.AddComponent(component.X, component.Y, component.Layout);
		}

		public void RemoveComponent(Model.IComponentModel component)
		{
			// TODO: implement removal of individual components in the case of performance issues
			GenerateModules();
			_weaponClasses.RemoveComponent(component.X, component.Y, component.Layout);
		}

		public void GenerateWeaponClasses()
		{
			_weaponClasses.Cleanup();
			if (_shipLayout == null) return;
			_weaponClasses.Initialize(_cellSize, new LayoutAdapter(_shipLayout));
			foreach (var item in _shipLayout.Components)
				_weaponClasses.AddComponent(item.X, item.Y, item.Layout);
		}

		private void GenerateMesh()
		{
			_body.SetMesh(null);
			if (_shipLayout == null) return;

			var builder = new ShipMeshBuilder(_cellSize);
			builder.OuterCellColor = _outerCellColor;
			builder.InnerCellColor = _innerCellColor;
			builder.EngineCellColor = _engineCellColor;
			builder.WeaponCellColor = _weaponCellColor;
			builder.Build(new LayoutAdapter(_shipLayout));

			_body.SetMesh(builder.CreateMesh());
		}

		private void GenerateLockedCells()
		{
			_lockedCells.SetMesh(null);
			if (_shipLayout == null) return;

			_lockedCellBuilder = new(_cellSize, _shipLayout.Rect.xMin, _shipLayout.Rect.yMin, _lockSize);
			_lockedCellBuilder.Color = _lockedCellColor;

			foreach (var item in _shipLayout.Components)
				if (item.Locked)
					_lockedCellBuilder.TryAddElement(item.Layout, item.X, item.Y);

			_lockedCells.SetMesh(_lockedCellBuilder.CreateMesh());
		}

		private class CellValidator : SelectionMeshBuilder.ICellValidator
		{
			private readonly Model.IShipLayoutModel _model;
			private readonly GameDatabase.DataModel.Component _component;

			public CellValidator(Model.IShipLayoutModel model, GameDatabase.DataModel.Component component)
			{
				_model = model;
				_component = component;
			}

			public bool IsValid(int x, int y)
			{
				return _model.IsCellCompatible(x, y, _component);
			}

			public bool IsVisible(int x, int y)
			{
				return _model.Rect.IsInsideRect(x,y);
			}
		}

		private class LayoutAdapter : ShipMeshBuilder.ILayout
		{
			private readonly Model.IShipLayoutModel _model;
			public LayoutAdapter(Model.IShipLayoutModel model) => _model = model;
            public ref readonly LayoutRect Rect => ref _model.Rect;
            public CellType this[int x, int y] => _model.Cell(x,y);
			public string GetWeaponClasses(int x, int y) => _model.Barrel(x,y)?.WeaponClass;
		}
	}
}
