using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class StartupSplashOverlay : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindFirstObjectByType<StartupSplashOverlay>() != null) return;
        var host = new GameObject("BoundaryStudioStartupSplash", typeof(StartupSplashOverlay));
        DontDestroyOnLoad(host);
    }

    private IEnumerator Start()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var imageObject = new GameObject("FullScreenImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(transform, false);
        var rect = (RectTransform)imageObject.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = imageObject.GetComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = true;
        var texture = Resources.Load<Texture2D>("Textures/BoundaryStudio/loading_screen_preview17");
        if (texture != null)
        {
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            // The precomposed image must cover every aspect ratio.  Stretching
            // the white margins is visually lossless and avoids letterboxing.
            image.preserveAspect = false;
        }

        var hintObject = new GameObject("HeadphoneHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        hintObject.transform.SetParent(transform, false);
        var hintRect = (RectTransform)hintObject.transform;
        hintRect.anchorMin = new Vector2(0.08f, 0.04f);
        hintRect.anchorMax = new Vector2(0.92f, 0.14f);
        hintRect.offsetMin = hintRect.offsetMax = Vector2.zero;
        var hint = hintObject.GetComponent<Text>();
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 28;
        hint.resizeTextForBestFit = true;
        hint.resizeTextMinSize = 18;
        hint.resizeTextMaxSize = 34;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = Color.black;
        hint.raycastTarget = false;
        hint.text = "建议搭配耳机游玩获取最佳音频体验";
        yield return new WaitForSecondsRealtime(3f);
        Destroy(gameObject);
    }
}
