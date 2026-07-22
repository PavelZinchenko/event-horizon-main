using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    [DisallowMultipleComponent]
    public sealed class ReUIAmbientGraphic : MaskableGraphic
    {
        [SerializeField] private float _intensity = 1f;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();

            Color top = ReUIPalette.WithAlpha(ReUIPalette.AccentBlue, 0.025f * _intensity * color.a);
            Color bottom = ReUIPalette.WithAlpha(ReUIPalette.CanvasTint, 0.055f * _intensity * color.a);
            AddGradientQuad(vh, rect, top, bottom);

            float radius = Mathf.Min(rect.width, rect.height) * 0.48f;
            AddGlow(vh, new Vector2(rect.xMax - radius * 0.35f, rect.yMax - radius * 0.20f), radius,
                ReUIPalette.WithAlpha(ReUIPalette.AccentCyan, 0.030f * _intensity * color.a));
            AddGlow(vh, new Vector2(rect.xMin + radius * 0.20f, rect.yMin + radius * 0.10f), radius * 0.86f,
                ReUIPalette.WithAlpha(ReUIPalette.AccentPurple, 0.022f * _intensity * color.a));
        }

        private static void AddGradientQuad(VertexHelper vh, Rect rect, Color top, Color bottom)
        {
            int i = vh.currentVertCount;
            vh.AddVert(new Vector3(rect.xMin, rect.yMin), bottom, Vector2.zero);
            vh.AddVert(new Vector3(rect.xMin, rect.yMax), top, Vector2.up);
            vh.AddVert(new Vector3(rect.xMax, rect.yMax), top, Vector2.one);
            vh.AddVert(new Vector3(rect.xMax, rect.yMin), bottom, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        private static void AddGlow(VertexHelper vh, Vector2 center, float radius, Color centerColor)
        {
            const int segments = 32;
            int start = vh.currentVertCount;
            vh.AddVert(center, centerColor, new Vector2(0.5f, 0.5f));
            Color outer = ReUIPalette.WithAlpha(centerColor, 0f);

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(point, outer, Vector2.zero);
                if (i > 0)
                    vh.AddTriangle(start, start + i, start + i + 1);
            }
        }
    }
}
