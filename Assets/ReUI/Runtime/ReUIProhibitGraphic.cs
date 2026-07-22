using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// A single diagonal prohibition slash used by destructive "remove all"
    /// actions. It intentionally does not resemble the two-stroke close/exit X.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReUIProhibitGraphic : MaskableGraphic
    {
        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float size = Mathf.Min(rect.width, rect.height);
            Vector2 center = rect.center;
            Vector2 from = center + new Vector2(-0.25f, -0.25f) * size;
            Vector2 to = center + new Vector2(0.25f, 0.25f) * size;
            float halfWidth = size * 0.052f;
            Vector2 direction = (to - from).normalized;
            Vector2 normal = new(-direction.y, direction.x);
            Color slash = new(1f, 0.32f, 0.18f, color.a);

            int index = vh.currentVertCount;
            vh.AddVert(from - normal * halfWidth, slash, Vector2.zero);
            vh.AddVert(from + normal * halfWidth, slash, Vector2.up);
            vh.AddVert(to + normal * halfWidth, slash, Vector2.one);
            vh.AddVert(to - normal * halfWidth, slash, Vector2.right);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);

            AddCircle(vh, from, halfWidth, slash);
            AddCircle(vh, to, halfWidth, slash);
        }

        private static void AddCircle(VertexHelper vh, Vector2 center, float radius, Color color)
        {
            const int segments = 16;
            int start = vh.currentVertCount;
            vh.AddVert(center, color, new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segments; ++i)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(point, color, Vector2.zero);
                if (i > 0)
                    vh.AddTriangle(start, start + i, start + i + 1);
            }
        }
    }
}
