using System;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Ships.Modification
{
    public class LayoutModifications
    {
        public event Action DataChangedEvent = () => { };

        public LayoutModifications(Ship ship)
        {
            _layout = new(ship.Layout);
        }

        public Layout BuildLayout()
        {
            return _layout.BuildLayout();
        }

        public bool TryAddCell(int x, int y, CellType cellType)
        {
			if (!_layout.TryModifyCell(x, y, cellType))
				return false;

            DataChangedEvent.Invoke();

            return true;
        }

        public int TotalExtraCells()
        {
			return _layout.ModifiedCellCount(false);
        }

        public int ExtraCells()
        {
			return _layout.ModifiedCellCount(true);
        }

        public void Reset()
        {
            _layout.Reset();
            DataChangedEvent.Invoke();
        }

        public void Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Reset();
                return;
            }

			_layout.Deserialize(data);

            DataChangedEvent.Invoke();
        }

        public IEnumerable<byte> Serialize()
        {
			return _layout.Serialize();
        }

		public bool IsCellValid(int x, int y, CellType type)
		{
			return _layout.IsValidModification(x, y, type);
		}

		private readonly CustomLayout _layout;

		private readonly struct CustomLayout
		{
			public CustomLayout(Layout stockLayout)
			{
				_stockLayout = stockLayout;
				_layout = new char[_stockLayout.Size * _stockLayout.Size];

				Reset();
			}

			public bool IsCustomizable(int index) => _stockLayout.Data[index] == (char)CellType.Empty && _layout[index] != (char)CellType.Empty;

			public bool IsValidModification(int x, int y, CellType value)
			{
				var size = _stockLayout.Size;

				if (x < 0 || y < 0 || x >= size || y >= size)
					return false;

				var index = x + y * size;
				if (!IsCustomizable(index)) return false;

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

				_layout[x + y * _stockLayout.Size] = (char)value;
				return true;
			}

			public IEnumerable<byte> Serialize()
			{
				if (ModifiedCellCount(true) == 0)
					yield break;

				byte emptyCells = 0;
				for (var i = 0; i < _layout.Length; ++i)
				{
					var isEmpty = _layout[i] == (char)Layout.CustomizableCell || _layout[i] == (char)CellType.Empty || _layout[i] == _stockLayout.Data[i];
					if (isEmpty)
					{
						if (emptyCells == 0)
							yield return (byte)CellType.Empty;

						if (emptyCells == 0xff)
						{
							yield return emptyCells;
							emptyCells = 0;
							yield return (byte)CellType.Empty;
						}

						emptyCells++;
					}
					else
					{
						if (emptyCells > 0)
						{
							yield return emptyCells;
							emptyCells = 0;
						}

						var value = (byte)_layout[i];
						if (value == (byte)Layout.CustomWeaponCell)
							value = (byte)CellType.Weapon;

						yield return value;
					}
				}

				if (emptyCells > 0)
					yield return emptyCells;
			}

			public void Deserialize(byte[] data)
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

					TryModifyCell(x, y, (CellType)data[dataIndex]);

					index++;
					dataIndex++;
				}
			}

			public void Reset()
			{
				var size = _stockLayout.Size;

				for (var i = 0; i < size; ++i)
				{
					for (var j = 0; j < size; ++j)
					{
						var x = j;
						var y = i;

						var cellType = _stockLayout[x, y];
						if (cellType != (char)CellType.Empty)
						{
							_layout[i * size + j] = cellType;
						}
						else if (_stockLayout[x, y - 1] != (char)CellType.Empty || _stockLayout[x - 1, y] != (char)CellType.Empty ||
							_stockLayout[x + 1, y] != (char)CellType.Empty || _stockLayout[x, y + 1] != (char)CellType.Empty)
						{
							_layout[i * size + j] = (char)Layout.CustomizableCell;
						}
						else
						{
							_layout[i * size + j] = (char)CellType.Empty;
						}
					}
				}
			}

			public int ModifiedCellCount(bool skipEmpty)
			{
				var data = _stockLayout.Data;

				int count = 0;
				for (int i = 0; i < _layout.Length; i++)
				{
					var cell = _layout[i];
					if (cell == (char)Layout.CustomizableCell && skipEmpty) continue;
					if (cell == data[i]) continue;
					count++;
				}

				return count;
			}

			public Layout BuildLayout()
			{
				return new Layout(new string(_layout));
			}

			private readonly Layout _stockLayout;
			private readonly char[] _layout;
		}
	}
}
