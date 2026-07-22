using System;
using System.Collections.Generic;
using System.IO;
using Constructor.Ships;
using GameDatabase.DataModel;
using Services.Gui;
using Services.Resources;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShipEditor.UI
{
    /// <summary>
    /// A small self-contained artwork editor. It combines painting and decals:
    /// an opaque image replaces hull pixels while an image with alpha blends
    /// over them. The imported image is manipulated directly with touch.
    /// </summary>
    public sealed class ShipTextureCustomizationPanel : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        private ShipEditorWindow _owner;
        private UiSettings _uiSettings;
        private Sprite _baseSprite;
        private Texture2D _overlay;
        private Texture2D _preview;
        private RawImage _previewImage;
        private RawImage _overlayImage;
        private Text _status;
        private Canvas _canvas;
        private float _scaleValue = 1f;
        private float _rotationDegrees;
        private Vector2 _normalizedOffset;
        private bool _rotationDragging;
        private float _rotationStartAngle;
        private float _rotationStartValue;
        private RectTransform _rotationHandle;
        private readonly Dictionary<int, Vector2> _pointers = new Dictionary<int, Vector2>();
        private Vector2 _gestureCenter;
        private float _gestureDistance;
        private float _gestureAngle;

        public static void Open(ShipEditorWindow owner)
        {
            if (owner == null) return;
            var canvas = owner.GetComponentInParent<Canvas>() ??
                         owner.transform.root.GetComponentInChildren<Canvas>(true);
            if (canvas == null) return;

            var panelObject = new GameObject("ShipTextureCustomization", typeof(RectTransform));
            panelObject.transform.SetParent(canvas.transform, false);
            panelObject.transform.SetAsLastSibling();
            var panel = panelObject.AddComponent<ShipTextureCustomizationPanel>();
            var panelCanvas = panelObject.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 500;
            panelObject.AddComponent<GraphicRaycaster>();
            panel._canvas = canvas;
            panel.Initialize(owner);
        }

        private void Initialize(ShipEditorWindow owner)
        {
            _owner = owner;
            _uiSettings = owner.UiSettings;
            // Always start from the database hull slice.  A previously saved
            // override may have been produced from the complete sliced source
            // sheet by older builds and must never become the next edit mask.
            _baseSprite = owner.OriginalShipSprite;

            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var background = gameObject.AddComponent<Image>();
            background.color = ThemeBackground(0.97f);

            var title = CreateText("涂装编辑", 30);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-240, -70), new Vector2(240, -20));

            // The preview must have its own RectTransform.  Adding RawImage to
            // the panel root made SetRect resize the entire modal to 520x360;
            // the preview then intercepted every raycast, hiding the proper
            // editor page and making the Select Image button unreachable.
            var previewObject = new GameObject("ShipPreview", typeof(RectTransform), typeof(RawImage));
            previewObject.transform.SetParent(transform, false);
            _previewImage = previewObject.GetComponent<RawImage>();
            _previewImage.color = Color.white;
            // This graphic is the gesture surface. Pointer events bubble to
            // this panel, so every device gets the same album-style controls.
            _previewImage.raycastTarget = true;
            previewObject.AddComponent<Mask>().showMaskGraphic = true;

            var overlayObject = new GameObject("ImportedArtwork", typeof(RectTransform), typeof(RawImage));
            overlayObject.transform.SetParent(previewObject.transform, false);
            _overlayImage = overlayObject.GetComponent<RawImage>();
            _overlayImage.raycastTarget = false;
            _overlayImage.color = Color.white;
            _overlayImage.gameObject.SetActive(false);

            CreateButton("选择图片", new Vector2(-460, 190), new Vector2(-220, 245), SelectImage);
            CreateButton("应用", new Vector2(-130, 190), new Vector2(130, 245), Apply);
            CreateButton("还原原图", new Vector2(220, 190), new Vector2(460, 245), Restore);
            CreateAnchoredButton("返回", Vector2.up, Vector2.up,
                new Vector2(20, -82), new Vector2(180, -22), Close);

            _status = CreateText("请选择一张图片", 18);
            SetRect(_status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-460, 20), new Vector2(460, 55));
            var help = CreateText("单指拖动 · 双指缩放并旋转", 18);
            SetRect(help.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-360, -245), new Vector2(360, -205));
            CreateRotationHandle();
            RefreshBasePreview();
        }

        private void SelectImage()
        {
            var permission = NativeFilePicker.PickFileWithForcedPermission(path =>
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                try
                {
                    var bytes = File.ReadAllBytes(path);
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!texture.LoadImage(bytes, true))
                    {
                        Destroy(texture);
                        SetStatus("无法读取图片");
                        return;
                    }

                    if (_overlay != null) Destroy(_overlay);
                    _overlay = texture;
                    _scaleValue = 1f;
                    _rotationDegrees = 0f;
                    _normalizedOffset = Vector2.zero;
                    _overlayImage.texture = _overlay;
                    _overlayImage.gameObject.SetActive(true);
                    SetStatus("已载入：" + Path.GetFileName(path));
                    UpdateOverlayTransform();
                }
                catch (Exception error)
                {
                    SetStatus("导入失败：" + error.Message);
                }
            }, "image/*", "*/*");

            if (permission != NativeFilePicker.Permission.Granted)
                SetStatus("无法打开系统相册，请授予存储读取权限后重试");
        }

        private void Apply()
        {
            if (_overlay == null)
            {
                SetStatus("请先选择图片");
                return;
            }

            // Alpha blending covers both use cases: a fully opaque import is a
            // paint layer, while transparent artwork behaves like a decal.
            if (PlayerShipTextureOverrides.Apply(_owner.CurrentShipId, _baseSprite, _overlay,
                    true, _scaleValue, _normalizedOffset, _rotationDegrees, out var error))
            {
                _owner.RefreshShipArtwork();
                SetStatus("已保存，原始贴图仍保留");
            }
            else
                SetStatus("保存失败：" + error);
        }

        private void Restore()
        {
            PlayerShipTextureOverrides.Restore(_owner.CurrentShipId);
            _owner.RefreshShipArtwork();
            SetStatus("已还原原始贴图");
            RefreshBasePreview();
        }

        private void Close()
        {
            if (_overlay != null) Destroy(_overlay);
            if (_preview != null) Destroy(_preview);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            // The separate rotation handle remains available for one-finger
            // precision, while the hull itself is handled by EventSystem.
            if (_overlay == null || Input.touchCount > 1) return;
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && IsInsideRotationHandle(touch.position))
                {
                    _rotationDragging = true;
                    BeginRotation(touch.position);
                }
                if (_rotationDragging && touch.phase == TouchPhase.Moved)
                    UpdateRotation(touch.position);
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    _rotationDragging = false;
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && IsInsideRotationHandle(Input.mousePosition))
                {
                    _rotationDragging = true;
                    BeginRotation(Input.mousePosition);
                }
                if (_rotationDragging && Input.GetMouseButton(0))
                    UpdateRotation(Input.mousePosition);
                if (Input.GetMouseButtonUp(0))
                    _rotationDragging = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_overlay == null || !IsInsidePreview(eventData.position)) return;
            _pointers[eventData.pointerId] = eventData.position;
            RebasePointerGesture();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pointers.Remove(eventData.pointerId)) return;
            RebasePointerGesture();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_overlay == null || !_pointers.ContainsKey(eventData.pointerId)) return;
            _pointers[eventData.pointerId] = eventData.position;

            GetPointerGesture(out var center, out var distance, out var angle);
            TranslateByScreenDelta(center - _gestureCenter);
            if (_pointers.Count >= 2)
            {
                if (_gestureDistance > 0.01f)
                    _scaleValue = Mathf.Clamp(_scaleValue * distance / _gestureDistance, 0.1f, 8f);
                _rotationDegrees += Mathf.DeltaAngle(_gestureAngle, angle);
            }

            _gestureCenter = center;
            _gestureDistance = distance;
            _gestureAngle = angle;
            UpdateOverlayTransform();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_overlay == null || !IsInsidePreview(eventData.position)) return;
            _scaleValue = Mathf.Clamp(_scaleValue * Mathf.Pow(1.12f, eventData.scrollDelta.y), 0.1f, 8f);
            UpdateOverlayTransform();
        }

        private void RebasePointerGesture()
        {
            if (_pointers.Count == 0)
            {
                _gestureCenter = Vector2.zero;
                _gestureDistance = 0f;
                _gestureAngle = 0f;
                return;
            }
            GetPointerGesture(out _gestureCenter, out _gestureDistance, out _gestureAngle);
        }

        private void GetPointerGesture(out Vector2 center, out float distance, out float angle)
        {
            var enumerator = _pointers.Values.GetEnumerator();
            enumerator.MoveNext();
            var first = enumerator.Current;
            if (_pointers.Count < 2 || !enumerator.MoveNext())
            {
                center = first;
                distance = 0f;
                angle = 0f;
                return;
            }

            var second = enumerator.Current;
            center = (first + second) * 0.5f;
            distance = Vector2.Distance(first, second);
            var delta = second - first;
            angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        }

        private void RefreshBasePreview()
        {
            if (_preview != null) Destroy(_preview);
            _preview = PlayerShipTextureOverrides.CreateBasePreview(_baseSprite);
            _previewImage.texture = _preview;
            if (_preview != null)
            {
                _previewImage.uvRect = new Rect(0, 0, 1, 1);
                var maximum = new Vector2(780f, 440f);
                var factor = Mathf.Min(maximum.x / _preview.width, maximum.y / _preview.height);
                var size = new Vector2(_preview.width, _preview.height) * factor;
                var previewRect = _previewImage.rectTransform;
                previewRect.anchorMin = previewRect.anchorMax = new Vector2(0.5f, 0.5f);
                previewRect.pivot = new Vector2(0.5f, 0.5f);
                previewRect.anchoredPosition = new Vector2(0f, 5f);
                previewRect.sizeDelta = size;
            }
            UpdateOverlayTransform();
        }

        private void UpdateOverlayTransform()
        {
            if (_overlayImage == null || _preview == null || _overlay == null)
                return;

            var baseRect = _previewImage.rectTransform.rect;
            var overlayRect = _overlayImage.rectTransform;
            overlayRect.anchorMin = overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
            overlayRect.pivot = new Vector2(0.5f, 0.5f);
            overlayRect.sizeDelta = new Vector2(
                baseRect.width * _scaleValue * _overlay.width / _preview.width,
                baseRect.height * _scaleValue * _overlay.height / _preview.height);
            overlayRect.anchoredPosition = new Vector2(
                _normalizedOffset.x * baseRect.width,
                _normalizedOffset.y * baseRect.height);
            overlayRect.localEulerAngles = new Vector3(0f, 0f, _rotationDegrees);
        }

        private void CreateRotationHandle()
        {
            var handleObject = new GameObject("RotationHandle", typeof(RectTransform), typeof(Image));
            handleObject.transform.SetParent(transform, false);
            _rotationHandle = (RectTransform)handleObject.transform;
            _rotationHandle.anchorMin = _rotationHandle.anchorMax = new Vector2(0.5f, 0.5f);
            _rotationHandle.pivot = new Vector2(0.5f, 0.5f);
            _rotationHandle.anchoredPosition = new Vector2(435f, 5f);
            _rotationHandle.sizeDelta = new Vector2(116f, 58f);
            handleObject.GetComponent<Image>().color = ThemeButton();

            var label = CreateText("拖动旋转 ↻", 17);
            label.transform.SetParent(handleObject.transform, false);
            label.raycastTarget = false;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private bool IsInsideRotationHandle(Vector2 screenPosition)
        {
            if (_rotationHandle == null) return false;
            var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            return RectTransformUtility.RectangleContainsScreenPoint(_rotationHandle, screenPosition, camera);
        }

        private void BeginRotation(Vector2 screenPosition)
        {
            _rotationStartAngle = ScreenAngleAroundPreview(screenPosition);
            _rotationStartValue = _rotationDegrees;
        }

        private void UpdateRotation(Vector2 screenPosition)
        {
            _rotationDegrees = _rotationStartValue +
                               Mathf.DeltaAngle(_rotationStartAngle, ScreenAngleAroundPreview(screenPosition));
            UpdateOverlayTransform();
        }

        private float ScreenAngleAroundPreview(Vector2 screenPosition)
        {
            var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            var center = RectTransformUtility.WorldToScreenPoint(camera,
                _previewImage.rectTransform.TransformPoint(_previewImage.rectTransform.rect.center));
            var delta = screenPosition - center;
            return Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        }

        private bool IsInsidePreview(Vector2 screenPosition)
        {
            if (_previewImage == null) return false;
            var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            return RectTransformUtility.RectangleContainsScreenPoint(
                _previewImage.rectTransform, screenPosition, camera);
        }

        private void TranslateByScreenDelta(Vector2 screenDelta)
        {
            var rect = _previewImage.rectTransform;
            var camera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Vector2.zero, camera, out var origin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenDelta, camera, out var destination);
            var localDelta = destination - origin;
            var size = rect.rect.size;
            if (size.x > 0.01f && size.y > 0.01f)
                _normalizedOffset += new Vector2(localDelta.x / size.x, localDelta.y / size.y);
        }

        private void SetStatus(string value)
        {
            if (_status != null) _status.text = value;
        }

        private Text CreateText(string value, int size)
        {
            var objectValue = new GameObject("Text", typeof(RectTransform), typeof(Text));
            objectValue.transform.SetParent(transform, false);
            var text = objectValue.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = ThemeText();
            return text;
        }

        private Button CreateButton(string value, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var objectValue = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button));
            objectValue.transform.SetParent(transform, false);
            var button = objectValue.GetComponent<Button>();
            objectValue.GetComponent<Image>().color = ThemeButton();
            button.onClick.AddListener(action);
            var label = CreateText(value, 20);
            label.transform.SetParent(objectValue.transform, false);
            label.color = ThemeButtonText();
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetRect((RectTransform)objectValue.transform, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), min, max);
            return button;
        }

        private Button CreateAnchoredButton(string value, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var button = CreateButton(value, min, max, action);
            SetRect((RectTransform)button.transform, anchorMin, anchorMax, min, max);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private Color ThemeBackground(float alpha)
        {
            var color = _uiSettings == null ? DefaultPanelDeep : (Color)_uiSettings.BackgroundDark;
            color.a = alpha;
            return color;
        }

        private Color ThemeButton()
        {
            return _uiSettings == null ? DefaultButton : (Color)_uiSettings.ButtonColor;
        }

        private Color ThemeText()
        {
            return _uiSettings == null ? DefaultText : (Color)_uiSettings.TextColor;
        }

        private Color ThemeButtonText()
        {
            return _uiSettings == null ? DefaultText : (Color)_uiSettings.ButtonTextColor;
        }

        private static readonly Color DefaultPanelDeep = new Color(0.075f, 0.039f, 0.125f, 0.96f);
        private static readonly Color DefaultButton = new Color(0.545f, 0.361f, 0.965f, 1f);
        private static readonly Color DefaultText = new Color(0.847f, 0.769f, 1f, 1f);
    }
}
