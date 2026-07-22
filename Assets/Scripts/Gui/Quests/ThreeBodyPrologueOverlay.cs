using System;
using Domain.Quests;
using Gui.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Gui.Quests
{
    /// <summary>
    /// A scene-independent presentation layer for the illustrated ThreeBody
    /// prologue.  It deliberately follows the startup-splash pattern instead
    /// of living inside a quest window, because quest-window anchors differ
    /// between scenes and can crop a full-screen story image.
    /// </summary>
    public sealed class ThreeBodyPrologueOverlay : MonoBehaviour
    {
        public static void Show(string resourcePath, UserAction action, QuestEventSignal.Trigger trigger, Action afterAction = null)
        {
            var overlay = EnsureOverlay();
            overlay.SetPage(resourcePath, action, trigger, afterAction);
        }

        public static void Hide()
        {
            var overlay = _instance;
            _instance = null;
            if (overlay == null)
                return;

            // Unity destroys objects at the end of the frame.  Deactivate it
            // immediately so the next quest node cannot be blocked by the
            // outgoing page's touch target.
            overlay.gameObject.SetActive(false);
            Destroy(overlay.gameObject);
        }

        public static void HideUnlessPageTransitionPending()
        {
            var overlay = _instance;
            if (overlay == null || !overlay._awaitingReplacement)
                Hide();
        }

        private static ThreeBodyPrologueOverlay EnsureOverlay()
        {
            if (_instance != null)
                return _instance;

            var host = new GameObject("ThreeBodyPrologueOverlay", typeof(ThreeBodyPrologueOverlay));
            DontDestroyOnLoad(host);
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            BuildUi();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue - 1;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();

            var frame = CreateImage("StoryFrame", transform);
            Stretch((RectTransform)frame.transform, Vector2.zero, Vector2.one);
            frame.sprite = LoadPurpleFrameSprite();
            frame.color = Color.white;
            frame.raycastTarget = false;
            ConfigureCover(frame);

            _storyImage = CreateImage("StoryImage", transform);
            Stretch((RectTransform)_storyImage.transform, new Vector2(0.10f, 0.16f), new Vector2(0.90f, 0.84f));
            _storyImage.color = Color.white;
            _storyImage.preserveAspect = true;
            _storyImage.raycastTarget = false;

            var tapTarget = CreateImage("StoryTapTarget", transform);
            Stretch((RectTransform)tapTarget.transform, Vector2.zero, Vector2.one);
            tapTarget.color = new Color(1f, 1f, 1f, 0f);
            tapTarget.raycastTarget = true;
            _tapButton = tapTarget.gameObject.AddComponent<Button>();
            _tapButton.transition = Selectable.Transition.None;
        }

        private void SetPage(string resourcePath, UserAction action, QuestEventSignal.Trigger trigger, Action afterAction)
        {
            // A single overlay is deliberately retained while a page action
            // closes and recreates the underlying quest window.  Reusing it
            // avoids exposing the now-empty dialog for one frame between two
            // story images.
            _awaitingReplacement = false;
            _pageVersion++;
            _storyImage.sprite = LoadSprite(resourcePath);
            _tapButton.onClick.RemoveAllListeners();
            _tapButton.interactable = action != null && trigger != null;
            if (action == null || trigger == null)
                return;

            _tapButton.onClick.AddListener(() =>
            {
                // The original quest action button invokes the action and
                // then closes its dialog.  Keep this full-screen image alive
                // during that hand-off; the following page replaces it in
                // place, so no empty dialog can flash between pages.
                var pageVersion = _pageVersion;
                _tapButton.interactable = false;
                _awaitingReplacement = true;
                action.Invoke(trigger);
                afterAction?.Invoke();
                StartCoroutine(HideIfNoReplacement(pageVersion));
            });
        }

        private System.Collections.IEnumerator HideIfNoReplacement(int pageVersion)
        {
            // Keep the previous illustration on screen across the quest
            // window hand-off. This deliberately covers the transient empty
            // dialog that Unity creates before the next image page arrives.
            yield return new WaitForSecondsRealtime(0.35f);

            if (_instance == this && _pageVersion == pageVersion)
            {
                _awaitingReplacement = false;
                Hide();
            }
        }

        private static Image CreateImage(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<Image>();
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureCover(Image image)
        {
            if (image.sprite == null || image.sprite.texture == null)
                return;

            image.preserveAspect = false;
            var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite.texture.width / (float)image.sprite.texture.height;
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
                return sprite;

            var sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites != null && sprites.Length > 0)
                return sprites[0];

            var texture = Resources.Load<Texture2D>(resourcePath);
            return texture == null
                ? null
                : Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadPurpleFrameSprite()
        {
            if (_purpleFrameSprite != null)
                return _purpleFrameSprite;

            var source = Resources.Load<Texture2D>("Textures/BoundaryStudio/prologue_frame");
            if (source == null)
                return null;

            RenderTexture previous = RenderTexture.active;
            RenderTexture renderTexture = null;
            try
            {
                renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0,
                    RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;

                var converted = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
                {
                    name = "prologue_frame_purple",
                    filterMode = source.filterMode,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.DontUnloadUnusedAsset
                };
                converted.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);

                Color.RGBToHSV(ThreeBodyUiPalette.Accent, out var targetHue, out _, out _);
                var pixels = converted.GetPixels32();
                for (var i = 0; i < pixels.Length; i++)
                {
                    var color = (Color)pixels[i];
                    Color.RGBToHSV(color, out var hue, out var saturation, out var value);
                    if (saturation < 0.08f || hue < 0.43f || hue > 0.72f)
                        continue;

                    var alpha = color.a;
                    color = Color.HSVToRGB(targetHue, Mathf.Clamp01(Mathf.Max(0.30f, saturation)), value);
                    color.a = alpha;
                    pixels[i] = color;
                }

                converted.SetPixels32(pixels);
                converted.Apply(false, true);
                _purpleFrameSprite = Sprite.Create(converted,
                    new Rect(0, 0, converted.width, converted.height), new Vector2(0.5f, 0.5f), 100f);
                _purpleFrameSprite.name = "prologue_frame_purple";
                _purpleFrameSprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return _purpleFrameSprite;
            }
            catch (Exception error)
            {
                Debug.LogWarning("Unable to create purple prologue frame: " + error.Message);
                return LoadSprite("Textures/BoundaryStudio/prologue_frame");
            }
            finally
            {
                RenderTexture.active = previous;
                if (renderTexture != null)
                    RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private static ThreeBodyPrologueOverlay _instance;
        private static Sprite _purpleFrameSprite;

        private Image _storyImage;
        private Button _tapButton;
        private int _pageVersion;
        private bool _awaitingReplacement;
    }
}
