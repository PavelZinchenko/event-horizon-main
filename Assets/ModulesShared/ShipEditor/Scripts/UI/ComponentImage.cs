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

		public void SetIconFitted(Sprite icon, Color color, bool preserveAspect = false)
		{
			base.sprite = icon;
			base.color = color;
			base.preserveAspect = preserveAspect;
			SetDisplayRect(0f, 0f, 1f, 1f);
		}

		public void SetIcon(Sprite icon, string layout, int size, Color color)
		{
			base.sprite = icon;
			base.color = color;
			base.preserveAspect = false;

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

			var occupiedWidth = Mathf.Max(1, x1 - x0 + 1);
			var occupiedHeight = Mathf.Max(1, y1 - y0 + 1);
			var centerX = (x0 + x1 + 1f) * 0.5f / size;
			var centerY = 1f - (y0 + y1 + 1f) * 0.5f / size;
			var halfWidth = occupiedWidth * 0.5f / size;
			var halfHeight = occupiedHeight * 0.5f / size;

			SetDisplayRect(centerX - halfWidth, centerY - halfHeight,
				centerX + halfWidth, centerY + halfHeight);
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
			_minX = minX;
			_maxX = maxX;
			_minY = minY;
			_maxY = maxY;

			SetVerticesDirty();
		}
	}
}
