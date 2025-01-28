using System; 
using System.Linq;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using Constructor.Model;

namespace Constructor
{
	public class ShipLayoutObsolete
	{
		public ShipLayoutObsolete(IShipLayout layout, IEnumerable<Barrel> barrels, IEnumerable<IntegratedComponent> predefinedComponents)
		{
            InitializeLayout(layout);
		    Size = layout.Size;
		    CellCount = layout.CellCount;

            UpdateBarrelData(barrels);

			UsedSpace = 0;
			foreach (var component in predefinedComponents)
			{
                if (!component.Info) continue;

				try
				{
					var id = InstallComponent(component.Info, component.X, component.Y);
					if (id < 0)
					{
					    var moduleId = component.Info.Data != null ? component.Info.Data.Id.ToString() : "invalid ID";
                        GameDiagnostics.Trace.LogError($"invalid module installed: {moduleId}");

                        if (component.Info.Data != null)
                            GameDiagnostics.Trace.LogError($"invalid position - [{component.X},{component.Y}]");
                        else
                            GameDiagnostics.Trace.LogError("unknown module");
                        continue;
					}

					_components[id].KeyBinding = component.KeyBinding;
				    _components[id].Behaviour = component.Behaviour;
					_components[id].Locked = component.Locked;
				}
				catch (Exception e)
				{
				    UnityEngine.Debug.LogException(e);
                }
			}
		}

        private void InitializeLayout(IShipLayout shipLayout)
        {
            var size = shipLayout.Size;
            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    _layout.Add(new LayoutElement(shipLayout[j,i]));
        }

		public void Clear()
		{
			_components.Clear();
            for (int i = 0; i < _layout.Count; ++i)
            {
                var item = _layout[i];
                item.ComponentId = -1;
                _layout[i] = item;
            }
		}

		public int Size { get; private set; }
		public int CellCount { get; private set; }
		public int UsedSpace { get; private set; }

		public LayoutElement this[int x, int y]
		{
			get
			{
				if (x < 0 || y < 0 || x >= Size || y >= Size)
					return LayoutElement.Empty;

				return _layout[x + y*Size];
			}
		}

		public IEnumerable<IntegratedComponent> Components { get { return _components.Where(item => item != null); } }

		public IEnumerable<KeyValuePair<int,IntegratedComponent>> ComponentsIndex 
		{
			get
			{
				for (int i = 0; i < _components.Count; ++i)
				{
					var component = _components[i];
					if (component == null)
						continue;
					yield return new KeyValuePair<int, IntegratedComponent>(i, component);
				}
			} 
		}

		public int InstallComponent(ComponentInfo info, int x, int y, int desiredComponentId = -1)
		{
		    if (info.Data == null)
		        return -1;

			var componentSize = info.Data.Layout.Size;
			foreach (int index in LayoutIndices(info.Data.Layout, componentSize, x, y))
			{
				if (index < 0 || !CanInstall(info.Data, index))
					return -1;
			}

			var barrelId = -1;
		    var componentId = _components.Count;
		    if (desiredComponentId < 0 || desiredComponentId >= _components.Count || _components[desiredComponentId] != null)
                _components.Add(null);
            else
                componentId = desiredComponentId;

            foreach (int index in LayoutIndices(info.Data.Layout, componentSize, x, y))
			{
				var element = _layout[index];
				element.ComponentId = componentId;
				_layout[index] = element;
				UsedSpace++;

				if (element.BarrelId >= 0)
					barrelId = element.BarrelId;
			}

			_components[componentId] = new IntegratedComponent(info, x, y, barrelId, 0, 0, false);

			return componentId;
		}

		public IntegratedComponent GetComponent(int id)
		{
			return id < 0 || id > _components.Count ? null : _components[id];
		}

		public void RemoveComponent(IntegratedComponent item)
		{
			int id = _components.IndexOf(item);
			if (id < 0)
				return;

			RemoveComponent(id);
		}

		public void RemoveComponent(int id)
		{
			for (int i = 0; i < _layout.Count; ++i)
			{
				var element = _layout[i];
				if (element.ComponentId == id)
				{
					element.ComponentId = -1;
					_layout[i] = element;
					UsedSpace--;
				}
			}

			_components[id] = null;
		}

		public bool CanInstall(GameDatabase.DataModel.Component component, int x, int y)
		{
			if (x < 0 || x >= Size || y < 0 || y >= Size)
				return false;

			return CanInstall(component, y*Size + x);
		}

		private bool CanInstall(GameDatabase.DataModel.Component component, int index)
		{
			var element = _layout[index];
			if (element.ComponentId >= 0)
				return false;
			
			if (element.Type == CellType.Weapon && component.CellType == CellType.Weapon)
				return string.IsNullOrEmpty(element.WeaponClass) || component.WeaponSlotType == default || element.WeaponClass.Contains(component.WeaponSlotType);
			
			return component.CellType.CompatibleWith(element.Type);
		}

		private IEnumerable<int> LayoutIndices(Layout componentLayout, int componentSize, int x, int y)
		{
			for (int i = 0; i < componentSize; ++i)
			{
				for (int j = 0; j < componentSize; ++j)
				{
					var cellType = (CellType)componentLayout[j,i];
					if (cellType == CellType.Empty)
						continue;
					
					if (i + y < 0 || i + y >= Size || j + x < 0 || j + x >= Size)
						yield return -1;

					yield return (i+y)*Size + j+x;
				}
			}
		}

	    private void UpdateBarrelData(IEnumerable<Barrel> barrels)
	    {
	        var index = 0;
	        foreach (var barrel in barrels)
	        {
	            var item = _layout[barrel.PositionInLayout];
	            item.BarrelId = index++;
	            item.WeaponClass = barrel.WeaponClass;
	            _layout[barrel.PositionInLayout] = item;
	        }

	        for (var k = 0; k < 1000; ++k)
	        {
	            var shoulContinue = false;

	            for (var i = 0; i < Size; ++i)
	            {
	                for (var j = 0; j < Size; ++j)
	                {
	                    shoulContinue |= CheckCell(j, i);
	                    shoulContinue |= CheckCell(Size - j - 1, Size - i - 1);
	                }
	            }

	            if (!shoulContinue) break;
	        }
        }

        private bool CheckCell(int x, int y)
        {
            var index = y * Size + x;
            var item = _layout[index];

			if (item.Type != CellType.Weapon || item.BarrelId >= 0)
				return false;

            var dataChanged = false;

            if (x > 0 && TryAssignBarrelId(ref item, _layout[index - 1]))
                dataChanged = true;
            else if (x + 1 < Size && TryAssignBarrelId(ref item, _layout[index + 1]))
                dataChanged = true;
            else if (y > 0 && TryAssignBarrelId(ref item, _layout[index - Size]))
                dataChanged = true;
            else if (y + 1 < Size && TryAssignBarrelId(ref item, _layout[index + Size]))
                dataChanged = true;

            if (dataChanged) _layout[index] = item;

            return dataChanged;
        }

		private static bool TryAssignBarrelId(ref LayoutElement item, LayoutElement other)
		{
			if (other.Type == CellType.Weapon && !other.IsCustomWeaponCell && other.BarrelId >= 0)
			{
				item.BarrelId = other.BarrelId;
				item.WeaponClass = other.WeaponClass;
				return true;
			}

			return false;
		}

		private readonly List<IntegratedComponent> _components = new List<IntegratedComponent>();
        private readonly List<LayoutElement> _layout = new();

        public struct LayoutElement
		{
			public LayoutElement(CellType cellType)
			{
				_type = cellType;
				ComponentId = -1;
				BarrelId = -1;
				WeaponClass = null;
			}

			public CellType Type => IsCustomWeaponCell ? CellType.Weapon : _type;
			public bool IsCustomWeaponCell => _type == Layout.CustomWeaponCell;

			public int ComponentId;
			public int BarrelId;
			public string WeaponClass;

			private readonly CellType _type;

			public static readonly LayoutElement Empty = new(CellType.Empty);
		}
	}
}
