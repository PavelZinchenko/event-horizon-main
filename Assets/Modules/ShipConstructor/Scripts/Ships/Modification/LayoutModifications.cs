using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Ships.Modification
{
    public class LayoutModifications
    {
        public event Action DataChangedEvent;
        private readonly IShipLayout _shipLayout;
        private readonly CustomLayout _customLayout;

        public static bool IsDisabledForShip(Ship ship, ShipSettings shipSettings) => ship.CellsExpansions switch
        {
            ToggleState.Enabled => false,
            ToggleState.Disabled => true,
            _ => shipSettings?.DisableCellsExpansions ?? false,
        };

        public LayoutModifications(Ship ship, bool disableModifications = false)
        {
            if (disableModifications)
                _shipLayout = new ShipLayoutAdapter(ship.Layout);
            else
                _shipLayout = _customLayout = new(ship.Layout);
        }

        public IShipLayout BuildLayout()
        {
            return _shipLayout;
        }

        public bool TryAddCell(int x, int y, CellType cellType)
        {
			if (_customLayout == null || !_customLayout.TryModifyCell(x, y, cellType))
				return false;

            DataChangedEvent?.Invoke();

            return true;
        }

        public void FullyUpgrade()
        {
            _customLayout?.FullyUpgrade();
        }

        public int TotalExtraCells()
        {
			return _customLayout == null ? 0 : _customLayout.CustomizableCellCount;
        }

        public int ExtraCells()
        {
            return _customLayout == null ? 0 : _customLayout.AddedCellCount;
        }

        public void Reset()
        {
            _customLayout?.Reset();
            DataChangedEvent?.Invoke();
        }

        public void Deserialize(byte[] data)
        {
            if (_customLayout == null) return;

            if (data == null || data.Length == 0)
            {
                Reset();
                return;
            }

            if (!_customLayout.TryDeserialize(data))
                _customLayout.DeserializeObsolete(data);

            DataChangedEvent?.Invoke();
        }

        public IEnumerable<byte> Serialize()
        {
			return _customLayout?.Serialize() ?? Enumerable.Empty<byte>();
        }

		public bool IsCellValid(int x, int y, CellType type)
		{
			return _customLayout != null && _customLayout.IsValidModification(x, y, type);
		}

		private class CustomLayout : IShipLayout
		{
            private readonly Layout _stockLayout;
            private readonly char[] _layout;
            private readonly LayoutRect _rect;
            private int _customizableCellCount;
            private int _addedCellCount;

            public CustomLayout(Layout stockLayout)
			{
				_stockLayout = stockLayout;
                var size = _stockLayout.Size;
                var data = _stockLayout.Data;

                bool top = false, bottom = false, left = false, right = false;
                for (int i = 0; i < size; ++i)
                {
                    if (data[i] != (char)CellType.Empty) top = true;
                    if (data[size*size - i - 1] != (char)CellType.Empty) bottom = true;
                    if (data[i*size] != (char)CellType.Empty) left = true;
                    if (data[(i+1)*size - 1] != (char)CellType.Empty) right = true;
                }

                _rect = new LayoutRect(left ? -1 : 0, top ? -1 : 0, right ? size : size-1, bottom ? size : size-1);
				_layout = new char[_rect.Square];
				Reset();
			}

            public void FullyUpgrade()
            {
                for (int y = _rect.yMin; y <= _rect.yMax; ++y)
                {
                    for (int x = _rect.xMin; x <= _rect.xMax; ++x)
                    {
                        var index = _rect.ToArrayIndex(x, y);
                        if (_layout[index] != (char)Layout.CustomizableCell) continue;

                        var l = (CellType)_stockLayout[x - 1, y];
                        var r = (CellType)_stockLayout[x + 1, y];
                        var t = (CellType)_stockLayout[x, y - 1];
                        var b = (CellType)_stockLayout[x, y + 1];

                        if (l == CellType.Weapon || r == CellType.Weapon || t == CellType.Weapon || b == CellType.Weapon)
                            _layout[index] = (char)Layout.CustomWeaponCell;
                        else if (l == CellType.Inner || r == CellType.Inner || t == CellType.Inner || b == CellType.Inner)
                            _layout[index] = (char)CellType.Inner;
                        else if (l == CellType.Engine || r == CellType.Engine || t == CellType.Engine || b == CellType.Engine)
                            _layout[index] = (char)CellType.Engine;
                        else
                            _layout[index] = (char)CellType.Outer;
                    }
                }
            }

            public bool IsValidModification(int x, int y, CellType value)
			{
                if (!_rect.IsInsideRect(x,y))
                    return false;

                if (!IsCustomizable(x, y))
                    return false;

				if (value == CellType.Outer)
					return true;

				var l = (CellType)_stockLayout[x - 1, y];
				var r = (CellType)_stockLayout[x + 1, y];
				var t = (CellType)_stockLayout[x, y - 1];
				var b = (CellType)_stockLayout[x, y + 1];

				return value == l || value == r || value == t || value == b;
			}

			public bool TryModifyCell(int x, int y, CellType value)
			{
				if (!IsValidModification(x, y, value)) return false;

				if (value == CellType.Weapon)
					value = Layout.CustomWeaponCell;

				_layout[_rect.ToArrayIndex(x, y)] = (char)value;
                _addedCellCount++;
				return true;
			}

			public IEnumerable<byte> Serialize()
			{
				if (_addedCellCount == 0)
					yield break;

                for (int i = _rect.yMin; i <= _rect.yMax; ++i)
                {
                    for (int j = _rect.xMin; j <= _rect.xMax; ++j)
                    {
                        if (!IsCustomizable(j, i)) continue;
                        yield return CellTypeConveter.ToByte((CellType)_layout[_rect.ToArrayIndex(j,i)]);
                    }
                }
            }

            public bool TryDeserialize(byte[] data)
            {
                Reset();

                if (data.Length == 0)
                    return true;

                var index = 0;
                for (int i = _rect.yMin; i <= _rect.yMax; ++i)
                {
                    for (int j = _rect.xMin; j <= _rect.xMax; ++j)
                    {
                        if (index >= data.Length) break;
                        if (_layout[_rect.ToArrayIndex(j, i)] != (char)Layout.CustomizableCell) continue;
                        if (!CellTypeConveter.TryConvert(data[index++], out var cellType))
                            return false;
                        if (cellType == CellType.Empty)
                            continue;
                        if (!TryModifyCell(j, i, cellType))
                            UnityEngine.Debug.LogError($"Invalid modification - {cellType} [{j},{i}]");
                    }
                }

                return true;
            }

            public void DeserializeObsolete(byte[] data)
			{
				Reset();

				var index = 0;
				var dataIndex = 0;
				var size = _stockLayout.Size;

				while (dataIndex < data.Length && index < _layout.Length)
				{
					if (data[dataIndex] == (byte)CellType.Empty)
					{
						dataIndex++;
						index += data[dataIndex++];
						continue;
					}

					var x = index % size;
					var y = index / size;

					if (!TryModifyCell(x, y, (CellType)data[dataIndex]))
                        UnityEngine.Debug.LogError($"Invalid modification [{x},{y}]");

					index++;
					dataIndex++;
				}
			}

			public void Reset()
			{
                _addedCellCount = 0;
                _customizableCellCount = 0;

				for (var i = _rect.yMin; i <= _rect.yMax; ++i)
				{
					for (var j = _rect.xMin; j <= _rect.xMax; ++j)
					{
						var x = j;
						var y = i;

                        var index = _rect.ToArrayIndex(x, y);
                        var cellType = _stockLayout[x, y];
						if (cellType != (char)CellType.Empty)
						{
							_layout[index] = cellType;
						}
						else if (_stockLayout[x, y - 1] != (char)CellType.Empty || _stockLayout[x - 1, y] != (char)CellType.Empty ||
							_stockLayout[x + 1, y] != (char)CellType.Empty || _stockLayout[x, y + 1] != (char)CellType.Empty)
						{
							_layout[index] = (char)Layout.CustomizableCell;
                            _customizableCellCount++;
						}
						else
						{
							_layout[index] = (char)CellType.Empty;
						}
					}
				}
			}

            public int AddedCellCount => _addedCellCount;
            public int CustomizableCellCount => _customizableCellCount;

            private bool IsCustomizable(int x, int y) => _stockLayout[x,y] == (char)CellType.Empty && _layout[_rect.ToArrayIndex(x,y)] != (char)CellType.Empty;
            public ref readonly LayoutRect Rect => ref _rect;
            public int CellCount => _stockLayout.CellCount + _addedCellCount;
            public int Size => _stockLayout.Size;
            public CellType this[int x, int y] => (CellType)_layout[_rect.ToArrayIndex(x, y)];

            private static class CellTypeConveter
            {
                public const byte EmptyCell = 0;
                public const byte OuterCell = 1;
                public const byte InnerCell = 2;
                public const byte EngineCell = 3;
                public const byte WeaponCell = 4;

                public static byte ToByte(CellType cellType)
                {
                    switch (cellType)
                    {
                        case CellType.Outer: 
                            return OuterCell;
                        case CellType.Inner: 
                            return InnerCell;
                        case CellType.Engine: 
                            return EngineCell;
                        case CellType.Weapon:
                        case Layout.CustomWeaponCell:
                            return WeaponCell;
                        case CellType.Empty:
                        case Layout.CustomizableCell:
                            return EmptyCell;
                        default:
                            throw new System.InvalidOperationException();
                    }
                }

                public static bool TryConvert(byte value, out CellType cellType)
                {
                    switch (value)
                    {
                        case OuterCell:
                            cellType = CellType.Outer;
                            return true;
                        case InnerCell:
                            cellType = CellType.Inner;
                            return true;
                        case EngineCell:
                            cellType = CellType.Engine;
                            return true;
                        case WeaponCell:
                            cellType = CellType.Weapon;
                            return true;
                        case EmptyCell:
                            cellType = CellType.Empty;
                            return true;
                        default:
                            cellType = CellType.Empty;
                            return false;
                    }
                }
            }
        }
	}
}
