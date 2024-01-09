using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Model;
using GameDatabase.Enums;

namespace ShipEditor
{
	public class LockedCellsMeshBuilder
	{
		private readonly float _cellSize;
		private readonly float _iconHalfSize;
		private readonly HashSet<ulong> _map = new();

		public Color Color { get; set; } = Color.white;

		public LockedCellsMeshBuilder(float cellSize, float iconSize = 0.5f)
		{
			_cellSize = cellSize;
			_iconHalfSize = iconSize / 2;
		}

		public bool TryAddElement(Layout layout, int x0, int y0)
		{
			var size = layout.Size;
			var result = false;

			for (int i = 0; i < size; ++i)
				for (int j = 0; j < size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						result |= _map.Add(Model.CellIndex.FromXY(x0 + j, y0 + i));

			return result;
		}

		public bool TryRemoveElement(Layout layout, int x0, int y0)
		{
			var size = layout.Size;
			var result = false;

			for (int i = 0; i < size; ++i)
				for (int j = 0; j < size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						result |= _map.Remove(Model.CellIndex.FromXY(x0 + j, y0 + i));

			return result;
		}

		public Mesh CreateMesh()
		{
			List<Vector3> vertices = new();
			List<Vector2> uv = new();
			List<int> triangles = new();

			foreach (ulong index in _map)
			{
				Model.CellIndex.GetXY(index, out int x, out int y);

				var v1 = AddVertex(vertices, x + 0.5f - _iconHalfSize, y + 0.5f - _iconHalfSize);
				var v2 = AddVertex(vertices, x + 0.5f + _iconHalfSize, y + 0.5f - _iconHalfSize);
				var v3 = AddVertex(vertices, x + 0.5f + _iconHalfSize, y + 0.5f + _iconHalfSize);
				var v4 = AddVertex(vertices, x + 0.5f - _iconHalfSize, y + 0.5f + _iconHalfSize);

				uv.Add(new Vector2(0, 1));
				uv.Add(new Vector2(1, 1));
				uv.Add(new Vector2(1, 0));
				uv.Add(new Vector2(0, 0));

				triangles.Add(v1);
				triangles.Add(v2);
				triangles.Add(v3);
				triangles.Add(v3);
				triangles.Add(v4);
				triangles.Add(v1);
			}

			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uv.ToArray();
			mesh.colors = Enumerable.Repeat(Color, vertices.Count).ToArray();

			return mesh;
		}

		private int AddVertex(List<Vector3> vertices, float x, float y)
		{
			var id = vertices.Count;
			vertices.Add(new Vector3(x * _cellSize, -y * _cellSize));
			return id;
		}
	}
}
