using UnityEngine;
using UnityEngine.UI;

namespace ReUI
{
    public enum ReUIIconKind
    {
        None,
        Fleet,
        Technology,
        ShipEditor,
        Equipment,
        Missions,
        Settings,
        Battle,
        StarMap,
        Store,
        Multiplayer,
        Encyclopedia,
        Back,
        Close,
        Skills,
        Faction,
        Captain,
        Undo,
        QuickBattle,
        NextEnemy,
    }

    [DisallowMultipleComponent]
    public sealed class ReUIIconGraphic : MaskableGraphic
    {
        [SerializeField] private ReUIIconKind _kind;

        public ReUIIconKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value) return;
                _kind = value;
                SetVerticesDirty();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_kind == ReUIIconKind.None) return;

            Rect r = GetPixelAdjustedRect();
            float size = Mathf.Min(r.width, r.height);
            Rect square = new(
                r.center.x - size * 0.5f,
                r.center.y - size * 0.5f,
                size,
                size);

            Color accent = ReUIPalette.AccentFor(_kind);
            Color accent2 = Color.Lerp(accent, ReUIPalette.TextPrimary, 0.28f);
            Color dark = new(0.015f, 0.035f, 0.080f, 0.94f);

            AddCircle(vh, square, new Vector2(0.5f, 0.5f), 0.47f, 32, ReUIPalette.WithAlpha(dark, color.a));
            AddRing(vh, square, new Vector2(0.5f, 0.5f), 0.44f, 0.018f, 32, ReUIPalette.WithAlpha(accent, 0.62f * color.a));
            AddCircle(vh, square, new Vector2(0.30f, 0.24f), 0.17f, 20, ReUIPalette.WithAlpha(accent, 0.14f * color.a));

            switch (_kind)
            {
                case ReUIIconKind.Fleet:
                    DrawFleet(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Technology:
                    DrawTechnology(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.ShipEditor:
                    DrawShipEditor(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Equipment:
                    DrawEquipment(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Missions:
                    DrawMissions(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Settings:
                    DrawSettings(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Battle:
                    DrawBattle(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.StarMap:
                    DrawStarMap(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Store:
                    DrawStore(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Multiplayer:
                    DrawMultiplayer(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Encyclopedia:
                    DrawEncyclopedia(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Back:
                    DrawBack(vh, square, accent2);
                    break;
                case ReUIIconKind.Undo:
                    DrawUndo(vh, square, accent2);
                    break;
                case ReUIIconKind.Close:
                    DrawClose(vh, square, accent2);
                    break;
                case ReUIIconKind.Skills:
                    DrawSkills(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Faction:
                    DrawFaction(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.Captain:
                    DrawCaptain(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.QuickBattle:
                    DrawQuickBattle(vh, square, accent, accent2);
                    break;
                case ReUIIconKind.NextEnemy:
                    DrawNextEnemy(vh, square, accent, accent2);
                    break;
            }
        }

        private void DrawFleet(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddTriangle(vh, r, new Vector2(0.50f, 0.78f), new Vector2(0.26f, 0.29f), new Vector2(0.50f, 0.40f), light);
            AddTriangle(vh, r, new Vector2(0.50f, 0.78f), new Vector2(0.74f, 0.29f), new Vector2(0.50f, 0.40f), accent);
            AddQuad(vh, r, new Vector2(0.43f, 0.22f), new Vector2(0.49f, 0.38f), ReUIPalette.AccentGold);
            AddQuad(vh, r, new Vector2(0.51f, 0.22f), new Vector2(0.57f, 0.38f), ReUIPalette.AccentGold);
            AddCircle(vh, r, new Vector2(0.20f, 0.68f), 0.045f, 12, accent);
            AddCircle(vh, r, new Vector2(0.80f, 0.68f), 0.045f, 12, ReUIPalette.AccentPurple);
        }

        private void DrawTechnology(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddEllipseRing(vh, r, new Vector2(0.5f, 0.5f), 0.30f, 0.13f, 0.018f, 28, accent);
            AddEllipseRing(vh, r, new Vector2(0.5f, 0.5f), 0.13f, 0.30f, 0.018f, 28, light);
            AddDiagonalEllipseRing(vh, r, new Vector2(0.5f, 0.5f), 0.31f, 0.12f, 0.018f, 28, ReUIPalette.AccentPurple);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.075f, 18, ReUIPalette.AccentGold);
        }

        private void DrawShipEditor(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddTriangle(vh, r, new Vector2(0.48f, 0.76f), new Vector2(0.29f, 0.35f), new Vector2(0.48f, 0.43f), light);
            AddTriangle(vh, r, new Vector2(0.48f, 0.76f), new Vector2(0.67f, 0.35f), new Vector2(0.48f, 0.43f), accent);
            AddLine(vh, r, new Vector2(0.58f, 0.23f), new Vector2(0.79f, 0.44f), 0.055f, ReUIPalette.AccentGold);
            AddRing(vh, r, new Vector2(0.76f, 0.47f), 0.09f, 0.035f, 18, ReUIPalette.AccentGold);
        }

        private void DrawEquipment(VertexHelper vh, Rect r, Color accent, Color light)
        {
            // Cargo hold / equipment storage. The previous gear silhouette read as a
            // sun at small sizes and was easily confused with research and settings.
            AddQuad(vh, r, new Vector2(0.25f, 0.31f), new Vector2(0.75f, 0.70f), ReUIPalette.WithAlpha(accent, 0.90f));
            AddLine(vh, r, new Vector2(0.25f, 0.70f), new Vector2(0.39f, 0.79f), 0.030f, light);
            AddLine(vh, r, new Vector2(0.75f, 0.70f), new Vector2(0.61f, 0.79f), 0.030f, light);
            AddLine(vh, r, new Vector2(0.39f, 0.79f), new Vector2(0.61f, 0.79f), 0.030f, light);
            AddLine(vh, r, new Vector2(0.50f, 0.31f), new Vector2(0.50f, 0.70f), 0.026f, light);
            AddLine(vh, r, new Vector2(0.25f, 0.51f), new Vector2(0.75f, 0.51f), 0.026f, light);
            AddQuad(vh, r, new Vector2(0.44f, 0.44f), new Vector2(0.56f, 0.56f), ReUIPalette.AccentGold);
        }

        private void DrawMissions(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddQuad(vh, r, new Vector2(0.29f, 0.25f), new Vector2(0.70f, 0.76f), ReUIPalette.WithAlpha(light, 0.92f));
            AddQuad(vh, r, new Vector2(0.36f, 0.60f), new Vector2(0.63f, 0.64f), ReUIPalette.WithAlpha(accent, 0.75f));
            AddQuad(vh, r, new Vector2(0.36f, 0.50f), new Vector2(0.58f, 0.54f), ReUIPalette.WithAlpha(accent, 0.75f));
            AddLine(vh, r, new Vector2(0.36f, 0.37f), new Vector2(0.44f, 0.29f), 0.035f, ReUIPalette.AccentGreen);
            AddLine(vh, r, new Vector2(0.44f, 0.29f), new Vector2(0.61f, 0.45f), 0.035f, ReUIPalette.AccentGreen);
        }

        private void DrawSettings(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddGear(vh, r, new Vector2(0.5f, 0.5f), 0.29f, 0.20f, 8, accent);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.105f, 20, light);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.052f, 16, new Color(0.02f, 0.05f, 0.10f, 1f));
        }

        private void DrawBattle(VertexHelper vh, Rect r, Color accent, Color light)
        {
            // Primary combat emblem. The previous thin crossed blades disappeared
            // against bright star fields at phone resolution. A filled cyan shield,
            // dark inset and thick white blades retain contrast on every background.
            Color shield = Color.Lerp(accent, new Color(0.10f, 0.88f, 1.00f, 1f), 0.58f);
            Color inset = new(0.015f, 0.045f, 0.090f, 0.96f);
            AddTriangle(vh, r, new Vector2(0.50f, 0.84f), new Vector2(0.20f, 0.55f), new Vector2(0.50f, 0.16f), shield);
            AddTriangle(vh, r, new Vector2(0.50f, 0.84f), new Vector2(0.50f, 0.16f), new Vector2(0.80f, 0.55f), shield);
            AddTriangle(vh, r, new Vector2(0.50f, 0.76f), new Vector2(0.29f, 0.54f), new Vector2(0.50f, 0.25f), inset);
            AddTriangle(vh, r, new Vector2(0.50f, 0.76f), new Vector2(0.50f, 0.25f), new Vector2(0.71f, 0.54f), inset);

            AddLine(vh, r, new Vector2(0.29f, 0.31f), new Vector2(0.70f, 0.72f), 0.095f, light);
            AddLine(vh, r, new Vector2(0.71f, 0.31f), new Vector2(0.30f, 0.72f), 0.095f, Color.white);
            AddTriangle(vh, r, new Vector2(0.70f, 0.72f), new Vector2(0.58f, 0.67f), new Vector2(0.66f, 0.59f), ReUIPalette.AccentGold);
            AddTriangle(vh, r, new Vector2(0.30f, 0.72f), new Vector2(0.34f, 0.59f), new Vector2(0.42f, 0.67f), ReUIPalette.AccentGold);
            AddRing(vh, r, new Vector2(0.50f, 0.50f), 0.105f, 0.040f, 20, ReUIPalette.AccentGold);
        }

        private void DrawQuickBattle(VertexHelper vh, Rect r, Color accent, Color light)
        {
            // Lightning plus motion trails communicates "quick" without crossed
            // blades, diagonal cancellation marks or any X-shaped silhouette.
            AddLine(vh, r, new Vector2(0.22f, 0.66f), new Vector2(0.38f, 0.66f), 0.030f, light);
            AddLine(vh, r, new Vector2(0.18f, 0.52f), new Vector2(0.34f, 0.52f), 0.030f, accent);
            AddLine(vh, r, new Vector2(0.24f, 0.38f), new Vector2(0.38f, 0.38f), 0.030f, light);
            AddTriangle(vh, r, new Vector2(0.58f, 0.82f), new Vector2(0.36f, 0.49f), new Vector2(0.55f, 0.49f), ReUIPalette.AccentGold);
            AddTriangle(vh, r, new Vector2(0.42f, 0.18f), new Vector2(0.66f, 0.53f), new Vector2(0.48f, 0.53f), Color.white);
        }

        private void DrawNextEnemy(VertexHelper vh, Rect r, Color accent, Color light)
        {
            // Conventional next/skip glyph: a single right arrow followed by an
            // end marker. No crossing strokes, so it cannot read as close/cancel.
            AddLine(vh, r, new Vector2(0.24f, 0.50f), new Vector2(0.64f, 0.50f), 0.065f, light);
            AddLine(vh, r, new Vector2(0.50f, 0.67f), new Vector2(0.67f, 0.50f), 0.065f, accent);
            AddLine(vh, r, new Vector2(0.50f, 0.33f), new Vector2(0.67f, 0.50f), 0.065f, accent);
            AddLine(vh, r, new Vector2(0.76f, 0.30f), new Vector2(0.76f, 0.70f), 0.060f, ReUIPalette.AccentGold);
        }

        private void DrawStarMap(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddEllipseRing(vh, r, new Vector2(0.5f, 0.5f), 0.31f, 0.18f, 0.018f, 32, accent);
            AddCircle(vh, r, new Vector2(0.5f, 0.5f), 0.09f, 20, ReUIPalette.AccentGold);
            AddCircle(vh, r, new Vector2(0.22f, 0.58f), 0.045f, 14, light);
            AddCircle(vh, r, new Vector2(0.78f, 0.42f), 0.055f, 14, ReUIPalette.AccentPurple);
            AddLine(vh, r, new Vector2(0.66f, 0.66f), new Vector2(0.78f, 0.78f), 0.018f, light);
            AddLine(vh, r, new Vector2(0.72f, 0.78f), new Vector2(0.78f, 0.72f), 0.018f, light);
        }

        private void DrawStore(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddQuad(vh, r, new Vector2(0.27f, 0.28f), new Vector2(0.73f, 0.66f), ReUIPalette.WithAlpha(accent, 0.88f));
            AddRing(vh, r, new Vector2(0.5f, 0.65f), 0.18f, 0.035f, 22, light);
            AddQuad(vh, r, new Vector2(0.31f, 0.61f), new Vector2(0.69f, 0.69f), light);
            AddCircle(vh, r, new Vector2(0.41f, 0.43f), 0.04f, 12, ReUIPalette.AccentGold);
            AddCircle(vh, r, new Vector2(0.59f, 0.43f), 0.04f, 12, ReUIPalette.AccentGold);
        }

        private void DrawMultiplayer(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddCircle(vh, r, new Vector2(0.38f, 0.61f), 0.11f, 20, light);
            AddCircle(vh, r, new Vector2(0.64f, 0.57f), 0.10f, 20, accent);
            AddEllipseRing(vh, r, new Vector2(0.38f, 0.34f), 0.18f, 0.12f, 0.045f, 24, light);
            AddEllipseRing(vh, r, new Vector2(0.64f, 0.33f), 0.17f, 0.11f, 0.045f, 24, accent);
            AddLine(vh, r, new Vector2(0.47f, 0.48f), new Vector2(0.55f, 0.48f), 0.028f, ReUIPalette.AccentGold);
        }

        private void DrawEncyclopedia(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddQuad(vh, r, new Vector2(0.23f, 0.28f), new Vector2(0.48f, 0.72f), ReUIPalette.WithAlpha(light, 0.92f));
            AddQuad(vh, r, new Vector2(0.52f, 0.28f), new Vector2(0.77f, 0.72f), ReUIPalette.WithAlpha(accent, 0.92f));
            AddLine(vh, r, new Vector2(0.50f, 0.28f), new Vector2(0.50f, 0.72f), 0.025f, ReUIPalette.AccentGold);
            AddLine(vh, r, new Vector2(0.28f, 0.59f), new Vector2(0.43f, 0.59f), 0.018f, accent);
            AddLine(vh, r, new Vector2(0.57f, 0.59f), new Vector2(0.72f, 0.59f), 0.018f, light);
        }

        private void DrawBack(VertexHelper vh, Rect r, Color light)
        {
            AddLine(vh, r, new Vector2(0.30f, 0.50f), new Vector2(0.72f, 0.50f), 0.065f, light);
            AddLine(vh, r, new Vector2(0.30f, 0.50f), new Vector2(0.47f, 0.68f), 0.065f, light);
            AddLine(vh, r, new Vector2(0.30f, 0.50f), new Vector2(0.47f, 0.32f), 0.065f, light);
        }

        private void DrawUndo(VertexHelper vh, Rect r, Color light)
        {
            // A 180-degree U-turn arrow is visually distinct from the straight
            // Back arrow used to leave the editor.
            const int segments = 18;
            Vector2 center = new(0.52f, 0.50f);
            Vector2 previous = new(
                center.x + Mathf.Cos(25f * Mathf.Deg2Rad) * 0.25f,
                center.y + Mathf.Sin(25f * Mathf.Deg2Rad) * 0.25f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(25f, 205f, i / (float)segments) * Mathf.Deg2Rad;
                Vector2 current = new(
                    center.x + Mathf.Cos(angle) * 0.25f,
                    center.y + Mathf.Sin(angle) * 0.25f);
                AddLine(vh, r, previous, current, 0.055f, light);
                previous = current;
            }

            AddLine(vh, r, new Vector2(0.28f, 0.39f), new Vector2(0.28f, 0.63f), 0.055f, light);
            AddLine(vh, r, new Vector2(0.28f, 0.39f), new Vector2(0.47f, 0.39f), 0.055f, light);
        }

        private void DrawClose(VertexHelper vh, Rect r, Color light)
        {
            AddLine(vh, r, new Vector2(0.32f, 0.32f), new Vector2(0.68f, 0.68f), 0.065f, light);
            AddLine(vh, r, new Vector2(0.68f, 0.32f), new Vector2(0.32f, 0.68f), 0.065f, light);
        }

        private void DrawSkills(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddLine(vh, r, new Vector2(0.50f, 0.26f), new Vector2(0.50f, 0.48f), 0.028f, light);
            AddLine(vh, r, new Vector2(0.50f, 0.48f), new Vector2(0.30f, 0.68f), 0.028f, accent);
            AddLine(vh, r, new Vector2(0.50f, 0.48f), new Vector2(0.70f, 0.68f), 0.028f, accent);
            AddCircle(vh, r, new Vector2(0.50f, 0.24f), 0.090f, 18, ReUIPalette.AccentGold);
            AddCircle(vh, r, new Vector2(0.29f, 0.70f), 0.105f, 18, light);
            AddCircle(vh, r, new Vector2(0.71f, 0.70f), 0.105f, 18, accent);
            AddRing(vh, r, new Vector2(0.50f, 0.48f), 0.085f, 0.028f, 18, ReUIPalette.AccentPurple);
        }

        private void DrawFaction(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddLine(vh, r, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.76f), 0.025f, accent);
            AddLine(vh, r, new Vector2(0.50f, 0.50f), new Vector2(0.27f, 0.30f), 0.025f, light);
            AddLine(vh, r, new Vector2(0.50f, 0.50f), new Vector2(0.73f, 0.30f), 0.025f, ReUIPalette.AccentPurple);
            AddCircle(vh, r, new Vector2(0.50f, 0.78f), 0.105f, 18, accent);
            AddCircle(vh, r, new Vector2(0.25f, 0.28f), 0.105f, 18, light);
            AddCircle(vh, r, new Vector2(0.75f, 0.28f), 0.105f, 18, ReUIPalette.AccentPurple);
            AddRing(vh, r, new Vector2(0.50f, 0.50f), 0.105f, 0.035f, 18, ReUIPalette.AccentGold);
        }

        private void DrawCaptain(VertexHelper vh, Rect r, Color accent, Color light)
        {
            AddCircle(vh, r, new Vector2(0.50f, 0.63f), 0.145f, 22, light);
            AddQuad(vh, r, new Vector2(0.34f, 0.66f), new Vector2(0.66f, 0.72f), accent);
            AddQuad(vh, r, new Vector2(0.43f, 0.73f), new Vector2(0.57f, 0.78f), ReUIPalette.AccentGold);
            AddEllipseRing(vh, r, new Vector2(0.50f, 0.31f), 0.25f, 0.17f, 0.055f, 24, accent);
            AddLine(vh, r, new Vector2(0.39f, 0.47f), new Vector2(0.31f, 0.30f), 0.045f, light);
            AddLine(vh, r, new Vector2(0.61f, 0.47f), new Vector2(0.69f, 0.30f), 0.045f, light);
        }

        private static Vector2 P(Rect r, Vector2 normalized)
        {
            return new Vector2(
                Mathf.Lerp(r.xMin, r.xMax, normalized.x),
                Mathf.Lerp(r.yMin, r.yMax, normalized.y));
        }

        private static void AddQuad(VertexHelper vh, Rect r, Vector2 min, Vector2 max, Color color)
        {
            Vector2 a = P(r, min);
            Vector2 b = P(r, max);
            int i = vh.currentVertCount;
            vh.AddVert(new Vector3(a.x, a.y), color, Vector2.zero);
            vh.AddVert(new Vector3(a.x, b.y), color, Vector2.up);
            vh.AddVert(new Vector3(b.x, b.y), color, Vector2.one);
            vh.AddVert(new Vector3(b.x, a.y), color, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        private static void AddTriangle(VertexHelper vh, Rect r, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            int i = vh.currentVertCount;
            vh.AddVert(P(r, a), color, Vector2.zero);
            vh.AddVert(P(r, b), color, Vector2.zero);
            vh.AddVert(P(r, c), color, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
        }

        private static void AddCircle(VertexHelper vh, Rect r, Vector2 center, float radius, int segments, Color color)
        {
            Vector2 c = P(r, center);
            float scale = Mathf.Min(r.width, r.height);
            int start = vh.currentVertCount;
            vh.AddVert(c, color, new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 p = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * scale;
                vh.AddVert(p, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(start, start + i, start + i + 1);
            }
        }

        private static void AddRing(VertexHelper vh, Rect r, Vector2 center, float radius, float thickness, int segments, Color color)
        {
            AddEllipseRing(vh, r, center, radius, radius, thickness, segments, color);
        }

        private static void AddEllipseRing(VertexHelper vh, Rect r, Vector2 center, float radiusX, float radiusY, float thickness, int segments, Color color)
        {
            Vector2 c = P(r, center);
            float scale = Mathf.Min(r.width, r.height);
            int start = vh.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 outer = c + new Vector2(direction.x * radiusX, direction.y * radiusY) * scale;
                Vector2 inner = c + new Vector2(
                    direction.x * Mathf.Max(0f, radiusX - thickness),
                    direction.y * Mathf.Max(0f, radiusY - thickness)) * scale;
                vh.AddVert(outer, color, Vector2.zero);
                vh.AddVert(inner, color, Vector2.zero);
                if (i == 0) continue;
                int a = start + (i - 1) * 2;
                int b = a + 1;
                int c0 = start + i * 2;
                int d = c0 + 1;
                vh.AddTriangle(a, c0, d);
                vh.AddTriangle(a, d, b);
            }
        }

        private static void AddDiagonalEllipseRing(VertexHelper vh, Rect r, Vector2 center, float radiusX, float radiusY, float thickness, int segments, Color color)
        {
            Vector2 c = P(r, center);
            float scale = Mathf.Min(r.width, r.height);
            float rotation = 45f * Mathf.Deg2Rad;
            int start = vh.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 outerRaw = new(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
                Vector2 innerRaw = new(
                    Mathf.Cos(angle) * Mathf.Max(0f, radiusX - thickness),
                    Mathf.Sin(angle) * Mathf.Max(0f, radiusY - thickness));
                Vector2 outer = Rotate(outerRaw, rotation) * scale + c;
                Vector2 inner = Rotate(innerRaw, rotation) * scale + c;
                vh.AddVert(outer, color, Vector2.zero);
                vh.AddVert(inner, color, Vector2.zero);
                if (i == 0) continue;
                int a = start + (i - 1) * 2;
                int b = a + 1;
                int c0 = start + i * 2;
                int d = c0 + 1;
                vh.AddTriangle(a, c0, d);
                vh.AddTriangle(a, d, b);
            }
        }

        private static Vector2 Rotate(Vector2 p, float radians)
        {
            float c = Mathf.Cos(radians);
            float s = Mathf.Sin(radians);
            return new Vector2(p.x * c - p.y * s, p.x * s + p.y * c);
        }

        private static void AddLine(VertexHelper vh, Rect r, Vector2 from, Vector2 to, float width, Color color)
        {
            Vector2 a = P(r, from);
            Vector2 b = P(r, to);
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new(-direction.y, direction.x);
            float half = width * Mathf.Min(r.width, r.height) * 0.5f;
            int i = vh.currentVertCount;
            vh.AddVert(a - normal * half, color, Vector2.zero);
            vh.AddVert(a + normal * half, color, Vector2.zero);
            vh.AddVert(b + normal * half, color, Vector2.zero);
            vh.AddVert(b - normal * half, color, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        private static void AddGear(VertexHelper vh, Rect r, Vector2 center, float outerRadius, float innerRadius, int teeth, Color color)
        {
            Vector2 c = P(r, center);
            float scale = Mathf.Min(r.width, r.height);
            int segments = teeth * 2;
            int start = vh.currentVertCount;
            vh.AddVert(c, color, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                Vector2 p = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * scale;
                vh.AddVert(p, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(start, start + i, start + i + 1);
            }
        }
    }
}
