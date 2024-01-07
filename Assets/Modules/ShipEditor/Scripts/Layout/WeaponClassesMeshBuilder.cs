using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GameDatabase.Model;
using GameDatabase.Enums;

namespace ShipEditor
{
	public class WeaponClassesMeshBuilder
	{
		private const float _maxTotalWidth = 0.9f;
		private readonly float _cellSize;
		private readonly Vector2 _letterMaxSize;
		private readonly HashSet<int> _map = new();
		private readonly Dictionary<char, Sprite> _letters = new();
		private readonly ShipMeshBuilder.ILayout _layout;

		public Color Color { get; set; } = Color.white;

		public WeaponClassesMeshBuilder(float cellSize, Vector2 letterMaxSize, ShipMeshBuilder.ILayout layout)
		{
			_layout = layout;
			_cellSize = cellSize;
			_letterMaxSize = letterMaxSize;

			for (int i = 0; i < layout.Height; ++i)
				for (int j = 0; j < layout.Width; ++j)
					if (TryGetWeaponClass(j, i, out _))
						_map.Add(Index(j, i));
		}

		private bool TryGetWeaponClass(int x, int y, out string weaponClass)
		{
			weaponClass = null;
			if (_layout[x, y] != CellType.Weapon) return false;
			weaponClass = _layout.GetWeaponClasses(x, y);
			return !string.IsNullOrEmpty(weaponClass);
		}

		public void AddLetter(char letter, Sprite sprite) => _letters.Add(char.ToUpper(letter), sprite);

		public bool TryAddElement(Layout layout, int x0, int y0)
		{
			var size = layout.Size;
			var result = false;

			for (int i = 0; i < size; ++i)
				for (int j = 0; j < size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						result |= _map.Remove(Index(x0 + j, y0 + i));

			return result;
		}

		public bool TryRemoveElement(Layout layout, int x0, int y0)
		{
			var size = layout.Size;
			var result = false;
			for (int i = 0; i < size; ++i)
				for (int j = 0; j < size; ++j)
					if ((CellType)layout[j, i] != CellType.Empty)
						if (TryGetWeaponClass(x0 + j, y0 + i, out _))
							result |= _map.Add(Index(x0 + j, y0 + i));

			return result;
		}

		public Mesh CreateMesh()
		{
			List<Vector3> vertices = new();
			List<Vector2> uv = new();
			List<int> triangles = new();

			foreach (int index in _map)
			{
				var x = index % _layout.Width;
				var y = index / _layout.Width;

				if (!TryGetWeaponClass(x, y, out var weaponClass))
					continue;

				var length = weaponClass.Length;
				var totalWidth = _letterMaxSize.x * length;
				var scale = totalWidth > _maxTotalWidth ? _maxTotalWidth / totalWidth : 1f;
				var width = scale * _letterMaxSize.x;
				var height = scale * _letterMaxSize.y;

				for (int i = 0; i < length; ++i)
				{
					if (!_letters.TryGetValue(char.ToUpper(weaponClass[i]), out var sprite))
						continue;

					var spriteRect = new SpriteRect(sprite);

					var x0 = x + 0.5f - width*length/2 + i*width;
					var x1 = x0 + width;
					var y0 = y + 0.5f - height/2;
					var y1 = y0 + height;

					var v1 = AddVertex(vertices, x0, y0);
					var v2 = AddVertex(vertices, x1, y0);
					var v3 = AddVertex(vertices, x1, y1);
					var v4 = AddVertex(vertices, x0, y1);

					uv.Add(spriteRect.TransformUV(new Vector2(0, 0)));
					uv.Add(spriteRect.TransformUV(new Vector2(1, 0)));
					uv.Add(spriteRect.TransformUV(new Vector2(1, 1)));
					uv.Add(spriteRect.TransformUV(new Vector2(0, 1)));

					triangles.Add(v1);
					triangles.Add(v2);
					triangles.Add(v3);
					triangles.Add(v3);
					triangles.Add(v4);
					triangles.Add(v1);
				}
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

		private int Index(int x, int y) => x + y * _layout.Width;
	}
}
