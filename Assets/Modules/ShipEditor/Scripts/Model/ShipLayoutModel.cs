using System.Collections.Generic;
using GameDatabase.Model;
using GameDatabase.Enums;
using GameDatabase.DataModel;
using Constructor;

namespace ShipEditor.Model
{
	public interface IShipLayoutModel
	{
		CellType Cell(int x, int y);
		int Width { get; }
		int Height { get; }

		IEnumerable<IComponentModel> Components { get; }
		Barrel Barrel(int x, int y);
		bool TryGetComponentAt(int x, int y, out IComponentModel component);
		bool IsCellCompatible(int x, int y, Component component);
	}

	public class ShipLayoutModel : IShipLayoutModel
	{
		private readonly List<ComponentModel> _components = new();
		private readonly Dictionary<int, IComponentModel> _filledCells = new();
		private readonly BarrelMapBuilder _barrelMap = new();
		private readonly ImmutableCollection<Barrel> _barrels;
		private readonly IComponentTracker _tracker;
		private readonly Layout _layout;

		public int Width => _layout.Size;
		public int Height => _layout.Size;

		public CellType Cell(int x, int y)
		{
			var cellType = (CellType)_layout[x, y];
			if (cellType == Layout.CustomWeaponCell) return CellType.Weapon;
			if (cellType == Layout.CustomizableCell) return CellType.Empty;
			return cellType;
		}

		public IEnumerable<IComponentModel> Components => _components;

		public Barrel Barrel(int x, int y)
		{
			var id = _barrelMap[x, y];
			return id >= 0 ? _barrels[id] : null;
		}

		public ShipLayoutModel(Layout layout, ImmutableCollection<Barrel> barrels, IComponentTracker tracker)
		{
			_layout = layout;
			_barrels = barrels;
			_barrelMap.Build(layout);
			_tracker = tracker;
		}

		public bool TryGetComponentAt(int x, int y, out IComponentModel component) => _filledCells.TryGetValue(Index(x, y), out component);
		public IComponentModel GetComponentAt(int x, int y) => _filledCells.TryGetValue(Index(x, y), out var item) ? item : null;
		public int GetBarrelId(IComponentModel component) => GetBarrelId(component.X, component.Y, component.Data.Layout);

		public IComponentModel FindComponent(int x, int y, ComponentInfo info)
		{
			foreach (var item in _components)
				if (item.X == x && item.Y == y && item.Info == info)
					return item;
			return null;
		}

		public bool HasComponent(IComponentModel component)
		{
			var id = component.Id;
			if (id < 0 || id >= _components.Count) return false;
			return _components[id] == component;
		}

		public void RemoveAll(bool keepLocked = true)
		{
			if (!keepLocked)
			{
				foreach (var item in _components)
					_tracker.OnComponentRemoved(item.Data);

				_components.Clear();
				_filledCells.Clear();
				return;
			}

			int i = 0;
			while (i < _components.Count)
			{
				var component = _components[i];
				if (component.Locked)
				{
					i++;
					continue;
				}

				RemoveComponent(component);
			}
		}

		public void UpdateComponent(IComponentModel component, ComponentSettings settings)
		{
			if (!HasComponent(component))
				throw new System.InvalidOperationException();

			var model = _components[component.Id];
			model.Settings = settings;

			_tracker.OnKeyBindingChanged(component.Data, settings.KeyBinding);
		}

		public void RemoveComponent(IComponentModel component)
		{
			if (!HasComponent(component))
				throw new System.InvalidOperationException();

			ClearCells(component.X, component.Y, component.Data.Layout);

			var id = component.Id;
			int lastId = _components.Count - 1;
			if (id != lastId)
			{
				var last = _components[lastId];
				_components[id] = last;
				last.Id = id;
			}

			_components.RemoveAt(lastId);
			_tracker.OnComponentRemoved(component.Data);
		}

		public IComponentModel InstallComponent(int x, int y, ComponentInfo component, ComponentSettings settings)
		{
			var id = _components.Count;
			var layout = component.Data.Layout;
			var model = new ComponentModel(id, x, y, component, settings);
			FillCells(x, y, layout, model);
			_components.Add(model);
			_tracker.OnComponentAdded(component.Data);
			_tracker.OnKeyBindingChanged(component.Data, settings.KeyBinding);
			return model;
		}

		public bool IsCellCompatible(int x, int y, Component component)
		{
			var size = _layout.Size;

			if (x < 0 || x >= size || y < 0 || y >= size)
				return false;

			var index = x + y * size;
			if (_filledCells.ContainsKey(index))
				return false;

			var cellType = Cell(x, y);
			if (cellType == CellType.Weapon && component.CellType == CellType.Weapon)
			{
				var requiredSlot = component.WeaponSlotType;
				if (requiredSlot == WeaponSlotType.Default) return true;

				var barrelId = _barrelMap[x, y];
				var barrel = _barrels[barrelId];

				return string.IsNullOrEmpty(barrel.WeaponClass) || barrel.WeaponClass.Contains((char)requiredSlot);
			}

			return component.CellType.CompatibleWith(cellType);
		}

		public bool IsSuitableLocation(int x, int y, Component component)
		{
			var layout = component.Layout;

			for (int i = 0; i < layout.Size; ++i)
			{
				for (int j = 0; j < layout.Size; ++j)
				{
					if ((CellType)layout[j, i] == CellType.Empty) continue;
					if (!IsCellCompatible(x + j, y + i, component)) return false;
				}
			}

			return true;
		}

		private void FillCells(int x, int y, Layout layout, IComponentModel component)
		{
			for (int i = 0; i < layout.Size; ++i)
				for (int j = 0; j < layout.Size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						_filledCells.Add(Index(j + x, i + y), component);
		}

		private void ClearCells(int x, int y, Layout layout)
		{
			for (int i = 0; i < layout.Size; ++i)
				for (int j = 0; j < layout.Size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						_filledCells.Remove(Index(j + x, i + y));
		}

		private int GetBarrelId(int x, int y, Layout layout)
		{
			for (int i = 0; i < layout.Size; ++i)
				for (int j = 0; j < layout.Size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						return _barrelMap[x + j, y + i];

			return -1;
		}

		private int Index(int x, int y) => x + y * _layout.Size;
	}
}
