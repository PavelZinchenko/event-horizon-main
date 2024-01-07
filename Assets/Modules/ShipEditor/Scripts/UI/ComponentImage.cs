using UnityEngine;
using UnityEngine.UI;
using GameDatabase.Enums;

namespace ShipEditor.UI
{
	public class ComponentImage : Image
	{
		private float _minX = 0;
		private float _maxX = 1;
		private float _minY = 0;
		private float _maxY = 1;

		public void SetIcon(Sprite icon, string layout, int size, Color color)
		{
			base.sprite = icon;
			base.color = color;

			int x0 = size, x1 = 0, y0 = size, y1 = 0;

			for (int i = 0; i < size; ++i)
			{
				for (int j = 0; j < size; ++j)
				{
					if ((CellType)layout[i * size + j] == CellType.Empty)
						continue;
					if (j < x0) x0 = j;
					if (j > x1) x1 = j;
					if (i < y0) y0 = i;
					if (i > y1) y1 = i;
				}
			}

			var x = -0.5f * (size - x0 - x1 - 1) / size;
			var y = 0.5f * (size - y0 - y1 - 1) / size;

			SetDisplayRect(x, y, x + 1, y + 1);
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			base.OnPopulateMesh(vertexHelper);
		
			var corner1 = Vector2.zero;
			var corner2 = Vector2.one;
		
			corner1 -= rectTransform.pivot;
			corner2 -= rectTransform.pivot;
		
			corner1.x *= rectTransform.rect.width;
			corner1.y *= rectTransform.rect.height;
			corner2.x *= rectTransform.rect.width;
			corner2.y *= rectTransform.rect.height;
		
			for (int i = 0; i < vertexHelper.currentVertCount; ++i)
			{
				var vertex = new UIVertex();
				vertexHelper.PopulateUIVertex(ref vertex, i);
				var x = (vertex.position.x - corner1.x)/(corner2.x - corner1.x);
				var y = (vertex.position.y - corner1.y)/(corner2.y - corner1.y);

				x = _minX + x*(_maxX - _minX);
				y = _minY + y*(_maxY - _minY);

				vertex.position = new Vector3(corner1.x + x*(corner2.x - corner1.x), corner1.y + y*(corner2.y - corner1.y), vertex.position.y);
				vertexHelper.SetUIVertex(vertex, i);
			}
		}

		private void SetDisplayRect(float minX, float minY, float maxX, float maxY)
		{
			_maxX = maxX;
			_minX = minX;
			_maxY = maxY;
			_minY = minY;

			SetVerticesDirty();
		}
	}
}
