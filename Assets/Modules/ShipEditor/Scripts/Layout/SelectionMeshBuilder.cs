using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Model;
using GameDatabase.Enums;

namespace ShipEditor
{
	public class SelectionMeshBuilder
	{
		public interface ICellValidator
		{
			bool IsVisible(int x, int y);
			bool IsValid(int x, int y);
		}

		private readonly List<Vector3> _vertices = new();
		private readonly List<Vector2> _uv = new();
		private readonly List<Color32> _colors = new();
		private readonly List<int> _triangles = new();
		private readonly float _cellSize;
		private readonly ICellValidator _cellValidator;

		public Color ValidCellColor { get; set; } = Color.white;
		public Color InvalidCellColor { get; set; } = Color.white;

		public SelectionMeshBuilder(ICellValidator cellValidator, float cellSize)
		{
			_cellValidator = cellValidator;
			_cellSize = cellSize;
		}

		public void Build(Layout layout, int x0, int y0)
		{
			var size = layout.Size;

			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					var x = x0 + j;
					var y = y0 + i;

					if ((CellType)layout[j, i] == CellType.Empty) continue;
					if (!_cellValidator.IsVisible(x, y)) continue;

					var color = _cellValidator.IsValid(x, y) ? ValidCellColor : InvalidCellColor;
					var v1 = GetVertex(x, y, color);
					var v2 = GetVertex(x + 1, y, color);
					var v3 = GetVertex(x + 1, y + 1, color);
					var v4 = GetVertex(x, y + 1, color);
					AddFace(v1, v2, v3, v4);
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

		private int GetVertex(int x, int y, Color color)
		{
			var id = _vertices.Count;
			_vertices.Add(new Vector3(x * _cellSize, -y*_cellSize, 0));
			_uv.Add(new Vector2(x % 2 == 0 ? 0f : 1f, y % 2 == 0 ? 0f : 1f));
			_colors.Add(color);
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
