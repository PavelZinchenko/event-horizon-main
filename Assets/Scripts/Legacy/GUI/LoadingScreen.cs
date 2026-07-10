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
		_shipNameText.text = "边界工作室";
		_loadingText.text = string.Empty;
	}

	private void OnSceneManagerStateChanged(State state)
	{
		_active = state == State.Loading;

		if (_active && _firstTime)
			Show();
		else if (!_coroutineRunning)
			StartCoroutine(UpdateVisibility(_delay));
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
        _shipIcon.gameObject.SetActive(false);
        _shipNameText.text = "边界工作室";
        _shipNameText.color = Color.black;
        _loadingText.gameObject.SetActive(false);

        if (_studioLogoSprite == null)
        {
            var texture = Resources.Load<Texture2D>(StudioLogoPath);
            if (texture != null)
                _studioLogoSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        if (_studioLogoSprite != null)
        {
            _shipSprite.gameObject.SetActive(true);
            _shipSprite.sprite = _studioLogoSprite;
            _shipSprite.color = Color.black;
            _shipSprite.preserveAspect = true;
        }
        else
        {
            _shipSprite.gameObject.SetActive(false);
        }

        _firstTime = false;
    }

    private void Hide()
    {
		_canvas.enabled = false;
    }

    private bool _firstTime = true;
    private readonly System.Random _random = new System.Random();
    private SceneManagerStateChangedSignal _sceneManagerStateChangedSignal;
    private LocalizationChangedSignal _localizationChangedSignal;
    private Sprite _studioLogoSprite;

    private const string StudioLogoPath = "Textures/BoundaryStudio/logo";
}
