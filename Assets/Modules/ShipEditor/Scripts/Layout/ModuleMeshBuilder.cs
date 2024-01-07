using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Model;
using GameDatabase.Enums;
using Services.Reources;

namespace ShipEditor
{
	public class ModuleMeshBuilder
	{
		private readonly IResourceLocator _resourceLocator;
		private readonly List<Vector3> _vertices = new();
		private readonly List<Vector2> _uv = new();
		private readonly List<Color> _colors = new();
		private readonly List<int> _triangles = new();
		private readonly float _cellSize;

		public ModuleMeshBuilder(IResourceLocator resourceLocator, float cellSize)
		{
			_cellSize = cellSize;
			_resourceLocator = resourceLocator;
		}

		public void AddComponent(int x, int y, GameDatabase.DataModel.Component component)
		{
			var layout = component.Layout;
			var color = (Color)component.Color;

			var sprite = _resourceLocator.GetSprite(component.Icon);
			var spriteRect = new SpriteRect(sprite);
			var rect = new ComponentRect(layout);

			var xmin = rect.xmin;
			var xmax = rect.xmax + 1;
			var ymin = rect.ymin;
			var ymax = rect.ymax + 1;

			var v1 = GetVertex(x + xmin, y + ymin);
			var v2 = GetVertex(x + xmax, y + ymin);
			var v3 = GetVertex(x + xmax, y + ymax);
			var v4 = GetVertex(x + xmin, y + ymax);

			_uv.Add(spriteRect.TransformUV(rect.GetUV(xmin, ymin)));
			_uv.Add(spriteRect.TransformUV(rect.GetUV(xmax, ymin)));
			_uv.Add(spriteRect.TransformUV(rect.GetUV(xmax, ymax)));
			_uv.Add(spriteRect.TransformUV(rect.GetUV(xmin, ymax)));

			_triangles.Add(v1);
			_triangles.Add(v2);
			_triangles.Add(v3);
			_triangles.Add(v3);
			_triangles.Add(v4);
			_triangles.Add(v1);

			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);
		}

		public Mesh CreateMesh()
		{
			var mesh = new Mesh();
			mesh.vertices = _vertices.ToArray();
			mesh.triangles = _triangles.ToArray();
			mesh.uv = _uv.ToArray();
			mesh.colors = _colors.ToArray();
			mesh.Optimize();

			//Debug.LogError($"{mesh.vertices.Length} vertices, {mesh.triangles.Length / 3} triangles");
			return mesh;
		}

		private int GetVertex(int x, int y)
		{
			int id = _vertices.Count;
			_vertices.Add(new Vector3(x * _cellSize, -y * _cellSize, 0));
			return id;
		}

		private struct SpriteRect
		{
			public float xmin;
			public float xmax;
			public float ymin;
			public float ymax;

			public Vector2 TransformUV(Vector2 uv) => new Vector2(xmin + (xmax - xmin) * uv.x, ymax + (ymin - ymax) * uv.y);

			public SpriteRect(Sprite sprite)
			{
				int count = sprite.uv.Length;

				xmax = xmin = sprite.uv[0].x;
				ymax = ymin = sprite.uv[0].y;

				for (int i = 1; i < count; ++i)
				{
					var point = sprite.uv[i];
					if (point.x < xmin) xmin = point.x;
					if (point.x > xmax) xmax = point.x;
					if (point.y < ymin) ymin = point.y;
					if (point.y > ymax) ymax = point.y;
				}
			}
		}

		private struct ComponentRect
		{
			public int xmin;
			public int xmax;
			public int ymin;
			public int ymax;

			public int Width => xmax >= xmin ? xmax - xmin + 1 : 0;
			public int Height => ymax >= ymin ? ymax - ymin + 1 : 0;

			public Vector2 GetUV(int x, int y) => new((x - xmin) / (float)Width, (y - ymin) / (float)Height);

			public ComponentRect(Layout layout)
			{
				var size = layout.Size;
				int count = 0;

				xmin = size;
				xmax = 0;
				ymin = size;
				ymax = 0;

				for (int i = 0; i < size; ++i)
				{
					for (int j = 0; j < size; ++j)
					{
						var cell = (CellType)layout[j,i];
						if (cell == CellType.Empty) continue;

						count++;
						if (j < xmin) xmin = j;
						if (j > xmax) xmax = j;
						if (i < ymin) ymin = i;
						if (i > ymax) ymax = i;
					}
				}
			}
		}
	}
}
