using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Model;
using GameDatabase.Enums;
using Services.Resources;

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

		public void AddComponent(int x, int y, GameDatabase.DataModel.Component component, Layout layout, int rotation)
		{
			var color = (Color)component.Color;

			var sprite = _resourceLocator.GetSprite(component.Icon);
			var spriteRect = SpriteRect.Create(sprite);
			var rect = new ComponentRect(layout);
			// Component art is authored on a square canvas.  The empty cells in
			// the layout are part of that canvas, so stretching it to the occupied
			// rectangle distorts horizontal and vertical ThreeBody equipment.
			// Keep the source aspect ratio and let the transparent canvas align it
			// with the occupied cells instead.
			var size = Mathf.Max(1, layout.Size);
			var aspect = spriteRect.Aspect;
			if ((rotation & 1) != 0)
				aspect = new Vector2(aspect.y, aspect.x);
			var halfWidth = size * _cellSize * 0.5f * aspect.x;
			var halfHeight = size * _cellSize * 0.5f * aspect.y;
            var centerX = (x + 0.5f * (rect.xmax + rect.xmin + 1)) * _cellSize;
            var centerY = (y + 0.5f * (rect.ymax + rect.ymin + 1)) * _cellSize;

            int index = _vertices.Count;
			_vertices.Add(new Vector3(centerX - halfWidth, -centerY + halfHeight, 0));
			_vertices.Add(new Vector3(centerX + halfWidth, -centerY + halfHeight, 0));
			_vertices.Add(new Vector3(centerX + halfWidth, -centerY - halfHeight, 0));
			_vertices.Add(new Vector3(centerX - halfWidth, -centerY - halfHeight, 0));

			var uv = GetRotatedUv(rotation);
			_uv.Add(spriteRect.TransformUV(uv[0]));
			_uv.Add(spriteRect.TransformUV(uv[1]));
			_uv.Add(spriteRect.TransformUV(uv[2]));
			_uv.Add(spriteRect.TransformUV(uv[3]));

			_triangles.Add(index);
			_triangles.Add(index+1);
			_triangles.Add(index+2);
			_triangles.Add(index+2);
			_triangles.Add(index+3);
			_triangles.Add(index);

			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);
		}

		private static Vector2[] GetRotatedUv(int rotation)
		{
			switch (rotation & 3)
			{
				case 1: return new[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1) };
				case 2: return new[] { new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0) };
				case 3: return new[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0) };
				default: return new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
			}
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

		private struct SpriteRect
		{
			public float xmin;
			public float xmax;
			public float ymin;
			public float ymax;

			public Vector2 TransformUV(Vector2 uv) => new(xmin + (xmax - xmin) * uv.x, ymax + (ymin - ymax) * uv.y);

			public Vector2 Aspect
			{
				get
				{
					var width = xmax - xmin;
					var height = ymax - ymin;
					var max = Mathf.Max(width, height);
					return max > 0 ? new Vector2(width / max, height / max) : Vector2.one;
				}
			}

            public static SpriteRect Create(Sprite sprite)
            {
                if (sprite.packed)
                    return FromSpriteShape(sprite);
                else
                    return FullRect;
            }

			private static SpriteRect FromSpriteShape(Sprite sprite)
			{
                int count = sprite.uv.Length;

                var xmax = sprite.uv[0].x;
                var ymax = sprite.uv[0].y;
                var xmin = xmax;
                var ymin = ymax;

                for (int i = 1; i < count; ++i)
                {
                    var point = sprite.uv[i];
                    if (point.x < xmin) xmin = point.x;
                    if (point.x > xmax) xmax = point.x;
                    if (point.y < ymin) ymin = point.y;
                    if (point.y > ymax) ymax = point.y;
                }

                return new SpriteRect { xmin = xmin, xmax = xmax, ymin = ymin, ymax = ymax };
            }

            private static SpriteRect FullRect => new SpriteRect { xmin = 0, ymin = 0, xmax = 1, ymax = 1 };
		}

		private struct ComponentRect
		{
			public int xmin;
			public int xmax;
			public int ymin;
			public int ymax;

			public int Width => xmax >= xmin ? xmax - xmin + 1 : 0;
			public int Height => ymax >= ymin ? ymax - ymin + 1 : 0;
			public int Size => Mathf.Max(Width, Height);

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
