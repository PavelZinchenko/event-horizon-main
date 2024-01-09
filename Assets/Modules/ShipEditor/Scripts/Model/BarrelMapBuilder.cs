using System.Collections.Generic;
using GameDatabase.Model;
using GameDatabase.Enums;

namespace ShipEditor.Model
{
	public class BarrelMapBuilder
	{
		private string _layout;
		private byte[] _map;
		private readonly Queue<int> _mapCells = new Queue<int>();

		public byte BarrelCount { get; private set; }
		public int Size { get; private set; }

		public int this[int x, int y]
		{
			get
			{
				if (x < 0 || x >= Size) return -1;
				if (y < 0 || y >= Size) return -1;
				return _map[x + y * Size] - 1;
			}
		}

		public void Build(Layout layout)
		{
			BarrelCount = 0;
			Size = layout.Size;
			_layout = layout.Data;
			_map = new byte[Size * Size];

			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					if (TryAssignBarrel(j, i, (byte)(BarrelCount + 1)))
					{
						BarrelCount++;
						ProcessCells();
					}
				}
			}
		}

		private void ProcessCells()
		{
			while (_mapCells.Count > 0)
			{
				var index = _mapCells.Dequeue();
				var y = index / Size;
				var x = index % Size;
				var barrel = _map[index];

				TryAssignBarrel(x - 1, y, barrel);
				TryAssignBarrel(x + 1, y, barrel);
				TryAssignBarrel(x, y + 1, barrel);
				TryAssignBarrel(x, y - 1, barrel);
			}
		}

		private bool TryAssignBarrel(int x, int y, byte barrelId)
		{
			if (x < 0 || x >= Size) return false;
			if (y < 0 || y >= Size) return false;

			int index = x + y * Size;
			if (_map[index] > 0) return false;
			if (!IsWeapon((CellType)_layout[index])) return false;

			_map[index] = barrelId;
			_mapCells.Enqueue(index);
			return true;
		}

		private static bool IsWeapon(CellType cellType) => cellType == CellType.Weapon || cellType == Layout.CustomWeaponCell;
	}
}
