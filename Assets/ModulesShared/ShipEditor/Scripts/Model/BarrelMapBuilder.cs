using System.Collections.Generic;
using GameDatabase.Model;
using GameDatabase.Enums;
using Constructor.Model;

namespace ShipEditor.Model
{
	public class BarrelMapBuilder
	{
		private IShipLayout _layout;
		private byte[] _map;
        private byte _barrelCount;
		private readonly Queue<int> _mapCells = new Queue<int>();

        public byte BarrelCount => _barrelCount;

		public int this[int x, int y]
		{
			get
			{
                if (!_layout.Rect.IsInsideRect(x,y)) return -1;
				return _map[_layout.Rect.ToArrayIndex(x, y)] - 1;
			}
		}

		public void Build(IShipLayout layout, int maxCount)
		{
			_layout = layout;
			_map = new byte[_layout.Rect.Square];

			for (int i = _layout.Rect.yMin; i <= _layout.Rect.yMax; ++i)
				for (int j = _layout.Rect.yMin; j <= _layout.Rect.yMax; ++j)
					if (TryAssignNewBarrel(j, i, maxCount))
						ProcessCells();
	    }

        private void ProcessCells()
		{
			while (_mapCells.Count > 0)
			{
				var index = _mapCells.Dequeue();
                _layout.Rect.ArrayIndexToXY(index, out int x, out int y);
				var barrel = _map[index];

				TryAssignBarrel(x - 1, y, barrel);
				TryAssignBarrel(x + 1, y, barrel);
				TryAssignBarrel(x, y + 1, barrel);
				TryAssignBarrel(x, y - 1, barrel);
			}
		}

        private bool TryAssignNewBarrel(int x, int y, int maxCount)
        {
            if (!TryGetUnassignedWeaponCell(x, y, out int index, out bool isStock) || !isStock) return false;
            if (_barrelCount >= maxCount) return false;

            _map[index] = ++_barrelCount;
            _mapCells.Enqueue(index);
            return true;
        }

        private bool TryAssignBarrel(int x, int y, byte barrelId)
		{
            if (!TryGetUnassignedWeaponCell(x, y, out int index, out bool isStock)) return false;
            if (isStock) _mapCells.Enqueue(index);
            _map[index] = barrelId;
            return true;
        }

        private bool TryGetUnassignedWeaponCell(int x, int y, out int index, out bool isStock)
        {
            isStock = true;
            index = _layout.Rect.ToArrayIndex(x, y);
            if (!_layout.Rect.IsInsideRect(x, y)) return false;
            if (_map[index] > 0) return false;

            switch (_layout[x,y])
            {
                case CellType.Weapon:
                    isStock = true;
                    return true;
                case Layout.CustomWeaponCell:
                    isStock = false;
                    return true;
                default:
                    return false;
            }
        }
    }
}
