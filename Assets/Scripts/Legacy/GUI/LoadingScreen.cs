using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GameDatabase;
using GameDatabase.Query;
using GameServices.SceneManager;
using Services.Localization;
using Services.Resources;
using Services.Settings;
using Zenject;

public class LoadingScreen : MonoBehaviour
{
	[SerializeField] private Canvas _canvas;
    [SerializeField] private Text _shipNameText;
    [SerializeField] private Image _shipSprite;
    [SerializeField] private Image _shipIcon;
    [SerializeField] private Image _background;
	[SerializeField] private Text _loadingText;
	[SerializeField] private float _delay = 0.1f;

	[Inject] private readonly IResourceLocator _resourceLocator;
    [Inject] private readonly IDatabase _database;
	[Inject] private readonly ILocalization _localization;
	[Inject] private readonly IGameSettings _settings;

	private bool _active;
	private bool _coroutineRunning;

	[Inject]
    private void Initialize(
        SceneManagerStateChangedSignal sceneManagerStateChangedSignal,
        LocalizationChangedSignal localizationChangedSignal)
    {
        _sceneManagerStateChangedSignal = sceneManagerStateChangedSignal;
		_sceneManagerStateChangedSignal.Event += OnSceneManagerStateChanged;
        _localizationChangedSignal = localizationChangedSignal;
        _localizationChangedSignal.Event += OnLocalizationChanged;
	}

	private void OnLocalizationChanged(string language)
	{
		// Preview 17 uses a single precomposed splash image.  Keeping text and
		// logo in separate UI elements made their placement depend on the scene
		// prefab and aspect ratio.
	}

	private void OnSceneManagerStateChanged(State state)
	{
		if (!_startupSplashComplete && state == State.Loading && !_startupSplashRunning)
			StartCoroutine(ShowStartupSplash());
		else if (!_startupSplashRunning)
			Hide();
	}

	private IEnumerator ShowStartupSplash()
	{
		_startupSplashRunning = true;
		Show();
		// This canvas appears after Unity's native splash.  Keep it visible for
		// exactly one startup presentation and never reuse it for later loads.
		yield return new WaitForSecondsRealtime(3f);
		_startupSplashComplete = true;
		_startupSplashRunning = false;
		Hide();
	}

	private IEnumerator UpdateVisibility(float delay)
	{
		_coroutineRunning = true;
		yield return new WaitForSecondsRealtime(delay);

		if (_active)
			Show();
		else
			Hide();

		_coroutineRunning = false;
	}

    private void Show()
    {
		_canvas.enabled = true;

        _background.gameObject.SetActive(true);
        _background.color = Color.white;
		var backgroundRect = _background.rectTransform;
		backgroundRect.anchorMin = Vector2.zero;
		backgroundRect.anchorMax = Vector2.one;
		backgroundRect.offsetMin = Vector2.zero;
		backgroundRect.offsetMax = Vector2.zero;
		backgroundRect.SetAsFirstSibling();

		if (_splashSprite == null)
		{
			var texture = Resources.Load<Texture2D>(SplashPath);
			if (texture != null)
				_splashSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
					new Vector2(0.5f, 0.5f), 100f);
		}
		if (_splashSprite != null)
		{
			_background.sprite = _splashSprite;
			ConfigureSplashCover(_background, _splashSprite.texture);
		}

		// The whole presentation is baked into the splash image so there is no
		// second layout pass capable of moving or tinting the studio mark.
		_shipSprite.gameObject.SetActive(false);
        _shipIcon.gameObject.SetActive(false);
		_shipNameText.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(false);

        _firstTime = false;
    }

    private void Hide()
    {
		_canvas.enabled = false;
	}

	private static void ConfigureSplashCover(Image image, Texture2D texture)
	{
		if (image == null || texture == null)
			return;

		image.preserveAspect = false;
		var fitter = image.GetComponent<AspectRatioFitter>() ?? image.gameObject.AddComponent<AspectRatioFitter>();
		fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
		fitter.aspectRatio = texture.width / (float)texture.height;
	}

    private bool _firstTime = true;
    private bool _startupSplashRunning;
    private bool _startupSplashComplete;
    private readonly System.Random _random = new System.Random();
    private SceneManagerStateChangedSignal _sceneManagerStateChangedSignal;
    private LocalizationChangedSignal _localizationChangedSignal;
    private Sprite _splashSprite;

    private const string SplashPath = "Textures/BoundaryStudio/loading_screen_preview17";
}
