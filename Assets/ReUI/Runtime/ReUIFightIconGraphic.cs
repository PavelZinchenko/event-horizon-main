using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// Dedicated high-contrast arena combat emblem. It intentionally does not use
    /// ReUIIconGraphic so generic icon cleanup and theme refreshes cannot suppress
    /// the primary fight action on device.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReUIFightIconGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            float scale = Mathf.Min(Mathf.Abs(r.width), Mathf.Abs(r.height));
            if (scale <= 0.01f) return;

            Color cyan = Tint(new Color(0.08f, 0.92f, 1.00f, 1f));
            Color cyanLight = Tint(new Color(0.76f, 0.98f, 1.00f, 1f));
            Color white = Tint(Color.white);
            Color gold = Tint(new Color(1.00f, 0.82f, 0.25f, 1f));
            Color dark = Tint(new Color(0.008f, 0.025f, 0.060f, 1f));

            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.45f, 44, cyan);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.365f, 44, dark);

            // Filled shield silhouette gives the emblem a broad, readable mass.
            AddTriangle(vh, r, new Vector2(0.50f, 0.82f), new Vector2(0.23f, 0.59f),
                new Vector2(0.50f, 0.18f), cyan);
            AddTriangle(vh, r, new Vector2(0.50f, 0.82f), new Vector2(0.50f, 0.18f),
                new Vector2(0.77f, 0.59f), cyan);
            AddTriangle(vh, r, new Vector2(0.50f, 0.72f), new Vector2(0.32f, 0.56f),
                new Vector2(0.50f, 0.29f), dark);
            AddTriangle(vh, r, new Vector2(0.50f, 0.72f), new Vector2(0.50f, 0.29f),
                new Vector2(0.68f, 0.56f), dark);

            // Crossed blades remain thick enough after phone-resolution scaling.
            AddLine(vh, r, new Vector2(0.28f, 0.29f), new Vector2(0.72f, 0.73f), 0.105f, white);
            AddLine(vh, r, new Vector2(0.72f, 0.29f), new Vector2(0.28f, 0.73f), 0.105f, cyanLight);
            AddTriangle(vh, r, new Vector2(0.72f, 0.73f), new Vector2(0.58f, 0.67f),
                new Vector2(0.67f, 0.58f), gold);
            AddTriangle(vh, r, new Vector2(0.28f, 0.73f), new Vector2(0.33f, 0.58f),
                new Vector2(0.42f, 0.67f), gold);

            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.105f, 24, gold);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.050f, 20, dark);
        }

        private Color Tint(Color source)
        {
            Color tint = color;
            return new Color(
                source.r * tint.r,
                source.g * tint.g,
                source.b * tint.b,
                source.a * tint.a);
        }

        private static Vector2 Point(Rect r, Vector2 normalized)
        {
            return new Vector2(
                Mathf.Lerp(r.xMin, r.xMax, normalized.x),
                Mathf.Lerp(r.yMin, r.yMax, normalized.y));
        }

        private static void AddCircle(
            VertexHelper vh,
            Rect r,
            Vector2 center,
            float radius,
            int segments,
            Color color)
        {
            Vector2 c = Point(r, center);
            float actualRadius = radius * Mathf.Min(Mathf.Abs(r.width), Mathf.Abs(r.height));
            int centerIndex = vh.currentVertCount;
            AddVertex(vh, c, color);
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                AddVertex(vh, c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * actualRadius, color);
            }

            for (int i = 0; i < segments; i++)
                vh.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + i + 2);
        }

        private static void AddTriangle(
            VertexHelper vh,
            Rect r,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Color color)
        {
            int index = vh.currentVertCount;
            AddVertex(vh, Point(r, a), color);
            AddVertex(vh, Point(r, b), color);
            AddVertex(vh, Point(r, c), color);
            vh.AddTriangle(index, index + 1, index + 2);
        }

        private static void AddLine(
            VertexHelper vh,
            Rect r,
            Vector2 from,
            Vector2 to,
            float thickness,
            Color color)
        {
            Vector2 a = Point(r, from);
            Vector2 b = Point(r, to);
            Vector2 direction = b - a;
            if (direction.sqrMagnitude <= 0.0001f) return;

            direction.Normalize();
            float halfWidth = thickness * Mathf.Min(Mathf.Abs(r.width), Mathf.Abs(r.height)) * 0.5f;
            Vector2 normal = new Vector2(-direction.y, direction.x) * halfWidth;
            int index = vh.currentVertCount;
            AddVertex(vh, a - normal, color);
            AddVertex(vh, a + normal, color);
            AddVertex(vh, b + normal, color);
            AddVertex(vh, b - normal, color);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void AddVertex(VertexHelper vh, Vector2 position, Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vh.AddVert(vertex);
        }
    }
}
