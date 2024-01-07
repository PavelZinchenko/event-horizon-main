using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Enums;

namespace ShipEditor
{
	public class ShipMeshBuilder
	{
		public interface ILayout
		{
			int Width { get; }
			int Height { get; }
			CellType this[int x, int y] { get; }
			string GetWeaponClasses(int x, int y);
		}

		private readonly Dictionary<int, int> _cache = new();
		private readonly List<Vector3> _vertices = new();
		private readonly List<Vector2> _uv = new();
		private readonly List<Color32> _colors = new();
		private readonly List<int> _triangles = new();
		private readonly float _cellSize;

		public Color OuterCellColor { get; set; } = Color.white;
		public Color InnerCellColor { get; set; } = Color.white;
		public Color EngineCellColor { get; set; } = Color.white;
		public Color WeaponCellColor { get; set; } = Color.white;

		public ShipMeshBuilder(float cellSize)
		{
			_cellSize = cellSize;
		}

		public void Build(ILayout layout)
		{
			for (int i = 0; i < layout.Height; ++i)
			{
				for (int j = 0; j < layout.Width; ++j)
				{
					var cellType1 = layout[j, i];
					var cellType2 = cellType1;
					var cellType3 = cellType1;
					var cellType4 = cellType1;

					var even = (i + j) % 2 == 0;

					if (!IsValidCell(cellType1)) continue;
					if (cellType1 == CellType.InnerOuter)
					{
						cellType1 = cellType2 = cellType3 = cellType4 = CellType.Inner;

						if (even)
						{
							cellType2 = CellType.Outer;
							cellType4 = CellType.Outer;
						}
						else
						{
							cellType1 = CellType.Outer;
							cellType3 = CellType.Outer;
						}
					}

					var v1 = GetVertex(j, i, cellType1);
					var v2 = GetVertex(j + 1, i, cellType2);
					var v3 = GetVertex(j + 1, i + 1, cellType3);
					var v4 = GetVertex(j, i + 1, cellType4);

					if (even)
						AddFace(v1, v2, v3, v4);
					else
						AddFace(v2, v3, v4, v1);
				}
			}
		}

		public Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.vertices = _vertices.ToArray();
			mesh.triangles = _triangles.ToArray();
			mesh.uv = _uv.ToArray();
			mesh.colors32 = _colors.ToArray();
			mesh.Optimize();

			//Debug.LogError($"{mesh.vertices.Length} vertices, {mesh.triangles.Length / 3} triangles");
			return mesh;
		}

		private static int Key(int x, int y, CellType cell)
		{
			return ((((y & 0xfff) << 12) + (x & 0xfff)) << 12) + (byte)cell;
		}

		private static bool IsValidCell(CellType cell)
		{
			switch (cell)
			{
				case CellType.Outer:
				case CellType.Inner:
				case CellType.InnerOuter:
				case CellType.Engine:
				case CellType.Weapon:
					return true;
				default:
					return false;
			}
		}

		private Color32 CellToColor(CellType cell)
		{
			switch (cell)
			{
				case CellType.Outer: return OuterCellColor;
				case CellType.Inner: return InnerCellColor;
				case CellType.InnerOuter: return InnerCellColor;
				case CellType.Engine: return EngineCellColor;
				case CellType.Weapon: return WeaponCellColor;
				default: return new Color32();
			}
		}

		private int GetVertex(int x, int y, CellType cell)
		{
			var key = Key(x, y, cell);
			if (!_cache.TryGetValue(key, out var id))
			{
				id = _vertices.Count;
				_vertices.Add(new Vector3(x * _cellSize, -y*_cellSize, 0));
				_uv.Add(new Vector2(x % 2 == 0 ? 0f : 1f, y % 2 == 0 ? 0f : 1f));
				_colors.Add(CellToColor(cell));
				_cache.Add(key, id);
			}

			return id;
		}

		private void AddFace(int v1, int v2, int v3, int v4)
		{
			_triangles.Add(v1);
			_triangles.Add(v2);
			_triangles.Add(v3);
			_triangles.Add(v3);
			_triangles.Add(v4);
			_triangles.Add(v1);
		}
	}
}
