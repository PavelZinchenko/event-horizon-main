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

        var titleRect = _shipNameText.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.anchoredPosition = new Vector2(130f, 0f);
        titleRect.sizeDelta = new Vector2(500f, 100f);
        _shipNameText.alignment = TextAnchor.MiddleLeft;
        _shipNameText.fontSize = 48;
        foreach (var effect in _shipNameText.GetComponents<BaseMeshEffect>())
            effect.enabled = false;

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
            // The supplied logo already contains its black mark on a white
            // field. Tinting it black turned the complete square into a block.
            _shipSprite.color = Color.white;
            _shipSprite.preserveAspect = true;
            var logoRect = _shipSprite.rectTransform;
            logoRect.anchorMin = logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(1f, 0.5f);
            logoRect.anchoredPosition = new Vector2(-20f, 0f);
            logoRect.sizeDelta = new Vector2(210f, 210f);
            logoRect.localRotation = Quaternion.identity;
            foreach (var effect in _shipSprite.GetComponents<BaseMeshEffect>())
                effect.enabled = false;
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
