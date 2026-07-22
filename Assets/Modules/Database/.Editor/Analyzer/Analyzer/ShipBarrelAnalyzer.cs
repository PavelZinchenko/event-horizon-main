using EditorDatabase;
using EditorDatabase.DataModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace Analyzer.Analyzer
{
	public class ShipBarrelAnalyzer
	{
		private readonly Database _database;

		public ShipBarrelAnalyzer(Database database)
		{
			_database = database;
		}

		public IEnumerable<string> AnalyzeShips()
		{
			var list = Enumerable.Empty<IItemId>();

			try
			{
				list = _database.GetItemList(typeof(Ship));
			}
			catch (DatabaseException e)
			{
				return Enumerable.Repeat(e.Message + "\n" + e.InnerException?.Message, 1);
			}

			return list.SelectMany(id => AnalyzeShip(_database.GetShip(id.Value))).Append("done");
		}

		private IEnumerable<string> AnalyzeShip(Ship ship)
		{
			_mapBuilder.Build(ship.Layout);

			int barrelCount = ship.Barrels == null ? 0 : ship.Barrels.Length;
			if (barrelCount != _mapBuilder.BarrelCount)
				yield return $"Number of barrels in {ship.Id} layout doesn't match Barrels array length ({_mapBuilder.BarrelCount} / {barrelCount})";

			for (int i = 0; i < barrelCount; ++i)
			{
				var position = ship.Barrels[i].Position;
				var barrelId = _mapBuilder.GetNearestBarrel(position) - 1;
				if (barrelId != i)
					yield return $"{ship.Id}: barrel {i} is probably located in the wrong place {barrelId}";
			}

			yield break;
		}

		private BarrelMapBuilder _mapBuilder = new();
	}

	public class BarrelMapBuilder
	{
		private string _layout;
		private int[] _map;
		private readonly Queue<int> _mapCells = new();

		public int Size { get; private set; }
		public int BarrelCount { get; private set; }
		public int this[int x, int y] 
		{
			get
			{
				if (x < 0 || x >= Size) return 0;
				if (y < 0 || y >= Size) return 0;
				return _map[x + y * Size];
			}
		}

		public int GetNearestBarrel(Vector2 position)
		{
			if (BarrelCount <= 1) return BarrelCount;

			var x = Math.Clamp((int)((1f - position.x) * 0.5f * (Size-1) + 0.5f), 0, Size-1);
			var y = Math.Clamp((int)((1f - position.y) * 0.5f * (Size-1) + 0.5f), 0, Size-1);

			int barrelId = this[x, y];
			if (barrelId > 0)
				return barrelId;

			int distance = Math.Max(Math.Max(x, Size - x - 1), Math.Max(y, Size - y - 1));
			for (int i = 1; i <= distance; ++i)
			{
				for (int j = -i; j <= i; ++j)
				{
					if ((barrelId = this[x+j, y+i]) > 0) return barrelId;
					if ((barrelId = this[x+j, y-i]) > 0) return barrelId;
					if ((barrelId = this[x+i, y+j]) > 0) return barrelId;
					if ((barrelId = this[x-i, y+j]) > 0) return barrelId;
				}
			}

			return 0;
		}

		public void Build(Layout layout)
		{
			BarrelCount = 0;
			Size = layout.Size;
			_layout = layout.Data;
			_map = new int[Size*Size];

			for (int i = 0; i < Size; ++i)
			{
				for (int j = 0; j < Size; ++j)
				{
					if (TryAssignBarrel(j, i, BarrelCount + 1))
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

		private bool TryAssignBarrel(int x, int y, int barrelId)
		{
			if (x < 0 || x >= Size) return false;
			if (y < 0 || y >= Size) return false;

			int index = x + y * Size;
			if (_map[index] > 0) return false;
			if (_layout[index] != (char)CellType.Weapon) return false;

			_map[index] = barrelId;
			_mapCells.Enqueue(index);
			return true;
		}
	}
}
