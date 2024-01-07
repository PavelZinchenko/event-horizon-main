using UnityEngine;
using GameDatabase.Enums;

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

		[SerializeField] private float _lockSize = 0.5f;

		private float _cellSize;

		public float Width => _shipLayout == null ? 0 : _shipLayout.Width * _cellSize;
		public float Height => _shipLayout == null ? 0 : _shipLayout.Height * _cellSize;

		public Vector3 ContentOffset => _content.localPosition;

		private LockedCellsMeshBuilder _lockedCellBuilder;
		private Model.IShipLayoutModel _shipLayout;
		private Vector2Int _selectedPosition;

		public void Initialize(Model.IShipLayoutModel layout, float cellSize)
		{
			_cellSize = cellSize;
			_shipLayout = layout;
			_selection.SetMesh(null);

			GenerateMesh();
			GenerateModules();
			GenerateWeaponClasses();

			_content.localPosition = new Vector3(-Width / 2, Height / 2, 0);
		}

		public void GenerateModules()
		{
			_modules.Initialize(_cellSize);
			if (_shipLayout == null) return;

			foreach (var item in _shipLayout.Components)
				_modules.AddComponent(item.X, item.Y, item.Data, false);

			_modules.UpdateMesh();

			GenerateLockedCells();
		}

		public void UpdateComponent(Model.IComponentModel component)
		{
			if (_lockedCellBuilder == null) return;

			bool result;
			if (component.Locked)
				result = _lockedCellBuilder.TryAddElement(component.Data.Layout, component.X, component.Y);
			else
				result = _lockedCellBuilder.TryRemoveElement(component.Data.Layout, component.X, component.Y);

			if (result)
				_lockedCells.SetMesh(_lockedCellBuilder.CreateMesh());
		}

		public void ShowSelection(Vector2Int position, GameDatabase.DataModel.Component component)
		{
			if (component == null || _shipLayout == null)
			{
				_selection.SetMesh(null);
				return;
			}

			if (position == _selectedPosition) return;
			_selectedPosition = position;

			var cellValidator = new CellValidator(_shipLayout, component);
			var builder = new SelectionMeshBuilder(cellValidator, _cellSize);
			builder.ValidCellColor = _validCellColor;
			builder.InvalidCellColor = _invalidCellColor;
			builder.Build(component.Layout, position.x, position.y);
			_selection.SetMesh(builder.CreateMesh());
		}

		public void AddComponent(Model.IComponentModel component)
		{
			_modules.AddComponent(component.X, component.Y, component.Data);
			_weaponClasses.AddComponent(component.X, component.Y, component.Data.Layout);
		}

		public void RemoveComponent(Model.IComponentModel component)
		{
			// TODO: implement removal of individual components in the case of performance issues
			GenerateModules();
			_weaponClasses.RemoveComponent(component.X, component.Y, component.Data.Layout);
		}

		public void GenerateWeaponClasses()
		{
			_weaponClasses.Cleanup();
			if (_shipLayout == null) return;
			_weaponClasses.Initialize(_cellSize, new LayoutAdapter(_shipLayout));
			foreach (var item in _shipLayout.Components)
				_weaponClasses.AddComponent(item.X, item.Y, item.Data.Layout);
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

			_lockedCellBuilder = new(_cellSize, _lockSize);
			_lockedCellBuilder.Color = _lockedCellColor;

			foreach (var item in _shipLayout.Components)
				if (item.Locked)
					_lockedCellBuilder.TryAddElement(item.Data.Layout, item.X, item.Y);

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
				return x >= 0 && y >= 0 && x < _model.Width && y < _model.Height;
			}
		}

		private class LayoutAdapter : ShipMeshBuilder.ILayout
		{
			private readonly Model.IShipLayoutModel _model;
			public LayoutAdapter(Model.IShipLayoutModel model) => _model = model;
			public CellType this[int x, int y] => _model.Cell(x,y);
			public int Width => _model.Width;
			public int Height => _model.Height;
			public string GetWeaponClasses(int x, int y) => _model.Barrel(x,y)?.WeaponClass;
		}
	}
}
