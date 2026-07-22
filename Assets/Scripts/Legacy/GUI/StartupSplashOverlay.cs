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
            ConfigureCover(image, texture);
        }
        yield return new WaitForSecondsRealtime(3f);
        Destroy(gameObject);
    }

    private static void ConfigureCover(Image image, Texture2D texture)
    {
        // `preserveAspect` alone fits a 16:9 asset inside a portrait screen,
        // leaving blank bands.  EnvelopeParent crops only the outer edges so
        // the splash fills every display while the logo and text keep their
        // original proportions.
        image.preserveAspect = false;
        var fitter = image.GetComponent<AspectRatioFitter>() ?? image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = texture.width / (float)texture.height;
    }
}
