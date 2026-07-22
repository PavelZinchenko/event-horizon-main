using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReUI
{
    /// <summary>
    /// A low-cost saturation/value colour square for the local ReUI theme picker.
    /// It uses one UGUI mesh instead of a generated texture or an extra shader.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReUIThemeColorSquareGraphic : Graphic
    {
        private const int Resolution = 24;
        private float _hue;

        internal void SetHue(float hue)
        {
            hue = Mathf.Repeat(hue, 1f);
            if (Mathf.Approximately(_hue, hue)) return;

            _hue = hue;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            for (int y = 0; y < Resolution; y++)
            {
                float value0 = (float)y / Resolution;
                float value1 = (float)(y + 1) / Resolution;
                float y0 = Mathf.Lerp(rect.yMin, rect.yMax, value0);
                float y1 = Mathf.Lerp(rect.yMin, rect.yMax, value1);

                for (int x = 0; x < Resolution; x++)
                {
                    float saturation0 = (float)x / Resolution;
                    float saturation1 = (float)(x + 1) / Resolution;
                    float x0 = Mathf.Lerp(rect.xMin, rect.xMax, saturation0);
                    float x1 = Mathf.Lerp(rect.xMin, rect.xMax, saturation1);

                    int index = vertexHelper.currentVertCount;
                    AddVertex(vertexHelper, new Vector2(x0, y0), saturation0, value0);
                    AddVertex(vertexHelper, new Vector2(x0, y1), saturation0, value1);
                    AddVertex(vertexHelper, new Vector2(x1, y1), saturation1, value1);
                    AddVertex(vertexHelper, new Vector2(x1, y0), saturation1, value0);
                    vertexHelper.AddTriangle(index, index + 1, index + 2);
                    vertexHelper.AddTriangle(index + 2, index + 3, index);
                }
            }
        }

        private void AddVertex(VertexHelper vertexHelper, Vector2 position, float saturation, float value)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = Color.HSVToRGB(_hue, saturation, value);
            vertex.uv0 = new Vector2(saturation, value);
            vertexHelper.AddVert(vertex);
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ReUIThemeColorSquareGraphic))]
    public sealed class ReUIThemeColorSquareInput : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        internal event Action<float, float> SelectionChanged;

        internal RectTransform Selection;
        internal float Saturation { get; private set; }
        internal float Brightness { get; private set; }

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        internal void SetSelection(float saturation, float brightness)
        {
            Saturation = Mathf.Clamp01(saturation);
            Brightness = Mathf.Clamp01(brightness);
            UpdateSelectorPosition();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        private void UpdateFromPointer(PointerEventData eventData)
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
            if (_rectTransform == null || eventData == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            Rect rect = _rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            Saturation = Mathf.Clamp01(Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x));
            Brightness = Mathf.Clamp01(Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y));
            UpdateSelectorPosition();
            SelectionChanged?.Invoke(Saturation, Brightness);
        }

        private void UpdateSelectorPosition()
        {
            if (Selection == null) return;

            Vector2 point = new(Saturation, Brightness);
            Selection.anchorMin = point;
            Selection.anchorMax = point;
            Selection.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>Horizontal hue strip used by the same theme picker.</summary>
    [DisallowMultipleComponent]
    public sealed class ReUIThemeHueStripGraphic : Graphic
    {
        private const int Resolution = 48;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            for (int index = 0; index < Resolution; index++)
            {
                float hue0 = (float)index / Resolution;
                float hue1 = (float)(index + 1) / Resolution;
                float x0 = Mathf.Lerp(rect.xMin, rect.xMax, hue0);
                float x1 = Mathf.Lerp(rect.xMin, rect.xMax, hue1);
                int start = vertexHelper.currentVertCount;

                AddVertex(vertexHelper, new Vector2(x0, rect.yMin), hue0);
                AddVertex(vertexHelper, new Vector2(x0, rect.yMax), hue0);
                AddVertex(vertexHelper, new Vector2(x1, rect.yMax), hue1);
                AddVertex(vertexHelper, new Vector2(x1, rect.yMin), hue1);
                vertexHelper.AddTriangle(start, start + 1, start + 2);
                vertexHelper.AddTriangle(start + 2, start + 3, start);
            }
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, float hue)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = Color.HSVToRGB(hue, 1f, 1f);
            vertex.uv0 = new Vector2(hue, 0.5f);
            vertexHelper.AddVert(vertex);
        }
    }
}
